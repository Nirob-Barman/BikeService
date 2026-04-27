using System.Text.Json;
using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Interfaces.Services;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;

namespace BikeService.Application.Services
{
    public class ServiceTicketService : IServiceTicketService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;
        private readonly IUserContextService _userContextService;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public ServiceTicketService(
            IUnitOfWork unitOfWork,
            IAuditLogService auditLogService,
            IUserContextService userContextService,
            INotificationService notificationService,
            IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
            _userContextService = userContextService;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        public async Task<Result<List<ServiceTicketDto>>> GetAllAsync(TicketFilterDto? filter = null)
        {
            var tickets = await _unitOfWork.Repository<ServiceTicket>()
                .GetAllWithIncludesAsync<ServiceTicket>(
                    t => t,
                    t => t.Bike,
                    t => t.Mechanic,
                    t => t.Items,
                    t => t.Invoices);

            if (filter != null)
            {
                if (filter.Status.HasValue)
                    tickets = tickets.Where(t => t.Status == filter.Status.Value);
                if (filter.MechanicId.HasValue)
                    tickets = tickets.Where(t => t.MechanicId == filter.MechanicId.Value);
                if (filter.DateFrom.HasValue)
                    tickets = tickets.Where(t => t.CreatedAt >= filter.DateFrom.Value);
                if (filter.DateTo.HasValue)
                    tickets = tickets.Where(t => t.CreatedAt <= filter.DateTo.Value);
                if (!string.IsNullOrEmpty(filter.CustomerId))
                    tickets = tickets.Where(t => t.Bike != null && t.Bike.CustomerId == filter.CustomerId);
            }

            var dtos = tickets.Select(ServiceTicketMapper.ToDto).ToList();
            return Result<List<ServiceTicketDto>>.Ok(dtos);
        }

        public async Task<Result<ServiceTicketDto>> GetByIdAsync(int id)
        {
            var tickets = await _unitOfWork.Repository<ServiceTicket>()
                .GetAllWithIncludesAsync<ServiceTicket>(
                    t => t.Id == id,
                    t => t,
                    t => t.Bike,
                    t => t.Mechanic,
                    t => t.Items,
                    t => t.Invoices);

            var ticket = tickets.FirstOrDefault();
            if (ticket == null)
                return Result<ServiceTicketDto>.Fail("Service ticket not found.");

            // Load item navigations separately
            var itemIds = ticket.Items.Select(i => i.Id).ToList();
            foreach (var item in ticket.Items)
            {
                if (item.ServiceTypeId.HasValue && item.ServiceType == null)
                {
                    item.ServiceType = await _unitOfWork.Repository<ServiceType>().GetByIdAsync(item.ServiceTypeId.Value);
                }
                if (item.PartId.HasValue && item.Part == null)
                {
                    item.Part = await _unitOfWork.Repository<Part>().GetByIdAsync(item.PartId.Value);
                }
            }

            return Result<ServiceTicketDto>.Ok(ServiceTicketMapper.ToDto(ticket));
        }

        public async Task<Result<List<ServiceTicketDto>>> GetMyTicketsAsync()
        {
            var customerId = _userContextService.UserId;
            if (string.IsNullOrEmpty(customerId))
                return Result<List<ServiceTicketDto>>.Fail("User not authenticated.");

            var tickets = await _unitOfWork.Repository<ServiceTicket>()
                .GetAllWithIncludesAsync<ServiceTicket>(
                    t => t.Bike != null && t.Bike.CustomerId == customerId,
                    t => t,
                    t => t.Bike,
                    t => t.Mechanic,
                    t => t.Items,
                    t => t.Invoices);

            var dtos = tickets.Select(ServiceTicketMapper.ToDto).ToList();
            return Result<List<ServiceTicketDto>>.Ok(dtos);
        }

        public async Task<Result<List<ServiceTicketDto>>> GetAssignedTicketsAsync()
        {
            var userId = _userContextService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Result<List<ServiceTicketDto>>.Fail("User not authenticated.");

            var mechanic = await _unitOfWork.Repository<Mechanic>()
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (mechanic == null)
                return Result<List<ServiceTicketDto>>.Fail("Mechanic profile not found for this account.");

            return await GetAllAsync(new TicketFilterDto { MechanicId = mechanic.Id });
        }

        public async Task<Result<int>> CreateAsync(ServiceTicketFormDto dto)
        {
            var bike = await _unitOfWork.Repository<CustomerBike>().GetByIdAsync(dto.BikeId);
            if (bike == null)
                return Result<int>.Fail("Bike not found.");

            if (dto.AppointmentId.HasValue)
            {
                var exists = await _unitOfWork.Repository<ServiceTicket>()
                    .AnyAsync(t => t.AppointmentId == dto.AppointmentId.Value);
                if (exists)
                    return Result<int>.Fail("A service ticket already exists for this appointment.");
            }

            if (dto.MechanicId.HasValue)
            {
                var mechanic = await _unitOfWork.Repository<Mechanic>().GetByIdAsync(dto.MechanicId.Value);
                if (mechanic == null)
                    return Result<int>.Fail("Mechanic not found.");
            }

            var ticket = ServiceTicketMapper.ToEntity(dto);
            ticket.CreatedBy = _userContextService.UserId;
            ticket.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<ServiceTicket>().AddAsync(ticket);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "ServiceTicket", "Create",
                _userContextService.UserId, _userContextService.Email,
                $"Created service ticket for bike ID {dto.BikeId}",
                entityId: ticket.Id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: null,
                newValues: JsonSerializer.Serialize(new
                {
                    ticket.BikeId,
                    ticket.MechanicId,
                    ticket.AppointmentId,
                    ticket.Status,
                    ticket.DiagnosisNotes,
                    ticket.EstimatedCompletionDate
                }));

            return Result<int>.Ok(ticket.Id, "Service ticket created successfully.");
        }

        public async Task<Result<bool>> UpdateStatusAsync(int id, ServiceTicketStatus newStatus)
        {
            var ticket = await _unitOfWork.Repository<ServiceTicket>().GetByIdAsync(id);
            if (ticket == null)
                return Result<bool>.Fail("Service ticket not found.");

            var currentStatus = ticket.Status;

            // Validate workflow transition
            if (newStatus == ServiceTicketStatus.Cancelled)
            {
                if (currentStatus == ServiceTicketStatus.Delivered)
                    return Result<bool>.Fail("Cannot cancel a delivered ticket.");
            }
            else
            {
                var validNext = GetNextStatus(currentStatus);
                if (validNext == null || validNext.Value != newStatus)
                    return Result<bool>.Fail($"Invalid status transition from {currentStatus} to {newStatus}.");
            }

            var oldValues = JsonSerializer.Serialize(new { Status = currentStatus.ToString() });

            // Handle stock deduction on InProgress
            if (newStatus == ServiceTicketStatus.InProgress)
            {
                await _unitOfWork.BeginTransaction();
                try
                {
                    await DeductPartsStockAsync(id);
                    ticket.Status = newStatus;
                    ticket.UpdatedBy = _userContextService.UserId;
                    ticket.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Repository<ServiceTicket>().Update(ticket);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();
                }
                catch
                {
                    await _unitOfWork.RollbackAsync();
                    throw;
                }
            }
            else if (newStatus == ServiceTicketStatus.Cancelled &&
                     (currentStatus == ServiceTicketStatus.InProgress ||
                      currentStatus == ServiceTicketStatus.QualityCheck ||
                      currentStatus == ServiceTicketStatus.ReadyForPickup))
            {
                await _unitOfWork.BeginTransaction();
                try
                {
                    await RestockPartsAsync(id);
                    ticket.Status = newStatus;
                    ticket.UpdatedBy = _userContextService.UserId;
                    ticket.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Repository<ServiceTicket>().Update(ticket);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();
                }
                catch
                {
                    await _unitOfWork.RollbackAsync();
                    throw;
                }
            }
            else
            {
                ticket.Status = newStatus;
                ticket.UpdatedBy = _userContextService.UserId;
                ticket.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<ServiceTicket>().Update(ticket);
                await _unitOfWork.SaveChangesAsync();
            }

            await _auditLogService.LogAsync(
                "ServiceTicket", "StatusUpdate",
                _userContextService.UserId, _userContextService.Email,
                $"Status changed from {currentStatus} to {newStatus} for ticket ID {id}",
                entityId: id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: JsonSerializer.Serialize(new { Status = newStatus.ToString() }));

            // Notify customer on ReadyForPickup
            if (newStatus == ServiceTicketStatus.ReadyForPickup)
            {
                var bikeRecord = await _unitOfWork.Repository<CustomerBike>().GetByIdAsync(ticket.BikeId);
                if (bikeRecord != null && !string.IsNullOrEmpty(bikeRecord.CustomerId))
                {
                    await _notificationService.CreateNotificationAsync(
                        bikeRecord.CustomerId,
                        "Bike Ready for Pickup",
                        $"Your bike service (ticket #{id}) is complete and ready for pickup.",
                        link: $"/ServiceTicket/Detail/{id}");
                }
            }

            return Result<bool>.Ok(true, $"Status updated to {newStatus}.");
        }

        public async Task<Result<bool>> AssignMechanicAsync(int id, int mechanicId)
        {
            var ticket = await _unitOfWork.Repository<ServiceTicket>().GetByIdAsync(id);
            if (ticket == null)
                return Result<bool>.Fail("Service ticket not found.");

            var mechanic = await _unitOfWork.Repository<Mechanic>().GetByIdAsync(mechanicId);
            if (mechanic == null)
                return Result<bool>.Fail("Mechanic not found.");

            var oldValues = JsonSerializer.Serialize(new { ticket.MechanicId });

            ticket.MechanicId = mechanicId;
            ticket.UpdatedBy = _userContextService.UserId;
            ticket.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<ServiceTicket>().Update(ticket);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "ServiceTicket", "AssignMechanic",
                _userContextService.UserId, _userContextService.Email,
                $"Assigned mechanic '{mechanic.FullName}' to ticket ID {id}",
                entityId: id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: JsonSerializer.Serialize(new { MechanicId = mechanicId }));

            return Result<bool>.Ok(true, "Mechanic assigned successfully.");
        }

        public async Task<Result<bool>> UpdateDiagnosisAsync(int id, string? notes, DateTime? estimatedCompletion)
        {
            var ticket = await _unitOfWork.Repository<ServiceTicket>().GetByIdAsync(id);
            if (ticket == null)
                return Result<bool>.Fail("Service ticket not found.");

            var oldValues = JsonSerializer.Serialize(new
            {
                ticket.DiagnosisNotes,
                ticket.EstimatedCompletionDate
            });

            ticket.DiagnosisNotes = notes;
            ticket.EstimatedCompletionDate = estimatedCompletion;
            ticket.UpdatedBy = _userContextService.UserId;
            ticket.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<ServiceTicket>().Update(ticket);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "ServiceTicket", "UpdateDiagnosis",
                _userContextService.UserId, _userContextService.Email,
                $"Updated diagnosis notes and estimated completion for ticket ID {id}",
                entityId: id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: JsonSerializer.Serialize(new
                {
                    DiagnosisNotes = notes,
                    EstimatedCompletionDate = estimatedCompletion
                }));

            return Result<bool>.Ok(true, "Diagnosis updated successfully.");
        }

        public async Task<Result<bool>> AddItemAsync(int ticketId, ServiceTicketItemFormDto dto)
        {
            if (!dto.ServiceTypeId.HasValue && !dto.PartId.HasValue)
                return Result<bool>.Fail("Either a service type or a part must be specified.");

            if (dto.Quantity < 1)
                return Result<bool>.FailField("Quantity", "Quantity must be at least 1.");

            var ticket = await _unitOfWork.Repository<ServiceTicket>().GetByIdAsync(ticketId);
            if (ticket == null)
                return Result<bool>.Fail("Service ticket not found.");

            if (ticket.Status == ServiceTicketStatus.Delivered || ticket.Status == ServiceTicketStatus.Cancelled)
                return Result<bool>.Fail("Cannot add items to a delivered or cancelled ticket.");

            decimal unitPrice = dto.UnitPrice;

            if (dto.PartId.HasValue)
            {
                var part = await _unitOfWork.Repository<Part>().GetByIdAsync(dto.PartId.Value);
                if (part == null)
                    return Result<bool>.Fail("Part not found.");
                if (part.StockQuantity < dto.Quantity)
                    return Result<bool>.Fail($"Insufficient stock. Available: {part.StockQuantity}.");
                if (unitPrice == 0)
                    unitPrice = part.UnitPrice;
            }

            if (dto.ServiceTypeId.HasValue)
            {
                var serviceType = await _unitOfWork.Repository<ServiceType>().GetByIdAsync(dto.ServiceTypeId.Value);
                if (serviceType == null)
                    return Result<bool>.Fail("Service type not found.");
                if (unitPrice == 0)
                    unitPrice = serviceType.BasePrice;
            }

            var item = ServiceTicketMapper.ItemToEntity(dto, ticketId);
            item.UnitPrice = unitPrice;
            item.CreatedBy = _userContextService.UserId;
            item.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<ServiceTicketItem>().AddAsync(item);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "ServiceTicketItem", "Create",
                _userContextService.UserId, _userContextService.Email,
                $"Added item to ticket ID {ticketId}",
                entityId: item.Id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: null,
                newValues: JsonSerializer.Serialize(new
                {
                    item.ServiceTicketId,
                    item.ServiceTypeId,
                    item.PartId,
                    item.Quantity,
                    item.UnitPrice
                }));

            return Result<bool>.Ok(true, "Item added successfully.");
        }

        public async Task<Result<bool>> RemoveItemAsync(int itemId)
        {
            var item = await _unitOfWork.Repository<ServiceTicketItem>().GetByIdAsync(itemId);
            if (item == null)
                return Result<bool>.Fail("Item not found.");

            var ticket = await _unitOfWork.Repository<ServiceTicket>().GetByIdAsync(item.ServiceTicketId);
            if (ticket != null &&
                (ticket.Status == ServiceTicketStatus.Delivered || ticket.Status == ServiceTicketStatus.Cancelled))
                return Result<bool>.Fail("Cannot remove items from a delivered or cancelled ticket.");

            var oldValues = JsonSerializer.Serialize(new
            {
                item.ServiceTicketId,
                item.ServiceTypeId,
                item.PartId,
                item.Quantity,
                item.UnitPrice
            });

            _unitOfWork.Repository<ServiceTicketItem>().Remove(item);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "ServiceTicketItem", "Delete",
                _userContextService.UserId, _userContextService.Email,
                $"Removed item ID {itemId} from ticket ID {item.ServiceTicketId}",
                entityId: itemId.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: null);

            return Result<bool>.Ok(true, "Item removed successfully.");
        }

        public async Task<Result<bool>> CancelAsync(int id)
        {
            return await UpdateStatusAsync(id, ServiceTicketStatus.Cancelled);
        }

        // --- Private helpers ---

        private static ServiceTicketStatus? GetNextStatus(ServiceTicketStatus current) => current switch
        {
            ServiceTicketStatus.Pending => ServiceTicketStatus.Diagnosed,
            ServiceTicketStatus.Diagnosed => ServiceTicketStatus.InProgress,
            ServiceTicketStatus.InProgress => ServiceTicketStatus.QualityCheck,
            ServiceTicketStatus.QualityCheck => ServiceTicketStatus.ReadyForPickup,
            ServiceTicketStatus.ReadyForPickup => ServiceTicketStatus.Delivered,
            _ => null
        };

        private async Task DeductPartsStockAsync(int ticketId)
        {
            var items = await _unitOfWork.Repository<ServiceTicketItem>()
                .Where(i => i.ServiceTicketId == ticketId && i.PartId.HasValue);

            foreach (var item in items)
            {
                var part = await _unitOfWork.Repository<Part>().GetByIdAsync(item.PartId!.Value);
                if (part == null) continue;
                part.StockQuantity -= item.Quantity;
                if (part.StockQuantity < 0) part.StockQuantity = 0;
                _unitOfWork.Repository<Part>().Update(part);
            }
        }

        private async Task RestockPartsAsync(int ticketId)
        {
            var items = await _unitOfWork.Repository<ServiceTicketItem>()
                .Where(i => i.ServiceTicketId == ticketId && i.PartId.HasValue);

            foreach (var item in items)
            {
                var part = await _unitOfWork.Repository<Part>().GetByIdAsync(item.PartId!.Value);
                if (part == null) continue;
                part.StockQuantity += item.Quantity;
                _unitOfWork.Repository<Part>().Update(part);
            }
        }
    }
}
