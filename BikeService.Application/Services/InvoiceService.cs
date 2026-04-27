using System.Text.Json;
using BikeService.Application.DTOs.Invoice;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Interfaces.Services;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;

namespace BikeService.Application.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;
        private readonly IUserContextService _userContextService;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        private const decimal TaxRate = 0.15m;

        public InvoiceService(
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

        public async Task<Result<List<InvoiceDto>>> GetAllAsync(InvoiceFilterDto? filter = null)
        {
            var invoices = await _unitOfWork.Repository<Invoice>()
                .GetAllWithIncludesAsync<Invoice>(
                    i => i,
                    i => i.ServiceTicket,
                    i => i.PromoCode,
                    i => i.PaymentTransactions);

            // Load nested bike for BikeSummary
            foreach (var inv in invoices)
            {
                if (inv.ServiceTicket != null && inv.ServiceTicket.Bike == null)
                {
                    inv.ServiceTicket.Bike = await _unitOfWork.Repository<CustomerBike>()
                        .GetByIdAsync(inv.ServiceTicket.BikeId);
                }
            }

            if (filter != null)
            {
                if (filter.Status.HasValue)
                    invoices = invoices.Where(i => i.Status == filter.Status.Value);
                if (filter.DateFrom.HasValue)
                    invoices = invoices.Where(i => i.CreatedAt >= filter.DateFrom.Value);
                if (filter.DateTo.HasValue)
                    invoices = invoices.Where(i => i.CreatedAt <= filter.DateTo.Value);
            }

            // Load gateway name for transactions
            foreach (var inv in invoices)
            {
                foreach (var tx in inv.PaymentTransactions)
                {
                    if (tx.Gateway == null)
                        tx.Gateway = await _unitOfWork.Repository<PaymentGateway>().GetByIdAsync(tx.GatewayId);
                }
            }

            var dtos = invoices.Select(InvoiceMapper.ToDto).ToList();
            return Result<List<InvoiceDto>>.Ok(dtos);
        }

        public async Task<Result<InvoiceDto>> GetByIdAsync(int id)
        {
            var invoice = await LoadInvoiceWithNavigationsAsync(id);
            if (invoice == null)
                return Result<InvoiceDto>.Fail("Invoice not found.");

            return Result<InvoiceDto>.Ok(InvoiceMapper.ToDto(invoice));
        }

        public async Task<Result<InvoiceDto>> GetByTicketIdAsync(int ticketId)
        {
            var invoice = await _unitOfWork.Repository<Invoice>()
                .FirstOrDefaultAsync(i => i.ServiceTicketId == ticketId);

            if (invoice == null)
                return Result<InvoiceDto>.Fail("Invoice not found for this ticket.");

            var loaded = await LoadInvoiceWithNavigationsAsync(invoice.Id);
            if (loaded == null)
                return Result<InvoiceDto>.Fail("Invoice not found.");

            return Result<InvoiceDto>.Ok(InvoiceMapper.ToDto(loaded));
        }

        public async Task<Result<List<InvoiceDto>>> GetMyInvoicesAsync()
        {
            var userId = _userContextService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Result<List<InvoiceDto>>.Fail("User is not authenticated.");

            var invoices = await _unitOfWork.Repository<Invoice>()
                .GetAllWithIncludesAsync<Invoice>(
                    i => i,
                    i => i.ServiceTicket,
                    i => i.PromoCode,
                    i => i.PaymentTransactions);

            foreach (var inv in invoices)
            {
                if (inv.ServiceTicket != null && inv.ServiceTicket.Bike == null)
                {
                    inv.ServiceTicket.Bike = await _unitOfWork.Repository<CustomerBike>()
                        .GetByIdAsync(inv.ServiceTicket.BikeId);
                }
            }

            var filtered = invoices.Where(i => i.ServiceTicket?.Bike?.CustomerId == userId);

            foreach (var inv in filtered)
            {
                foreach (var tx in inv.PaymentTransactions)
                {
                    if (tx.Gateway == null)
                        tx.Gateway = await _unitOfWork.Repository<PaymentGateway>().GetByIdAsync(tx.GatewayId);
                }
            }

            var dtos = filtered.Select(InvoiceMapper.ToDto).ToList();
            return Result<List<InvoiceDto>>.Ok(dtos);
        }

        public async Task<Result<InvoiceDto>> GetMyInvoiceByIdAsync(int id)
        {
            var userId = _userContextService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Result<InvoiceDto>.Fail("User is not authenticated.");

            var invoice = await LoadInvoiceWithNavigationsAsync(id);
            if (invoice == null)
                return Result<InvoiceDto>.Fail("Invoice not found.");

            if (invoice.ServiceTicket?.Bike?.CustomerId != userId)
                return Result<InvoiceDto>.Fail("Access denied.");

            return Result<InvoiceDto>.Ok(InvoiceMapper.ToDto(invoice));
        }

        public async Task<Result<int>> GenerateAsync(int ticketId)
        {
            var existingInvoice = await _unitOfWork.Repository<Invoice>()
                .AnyAsync(i => i.ServiceTicketId == ticketId);
            if (existingInvoice)
                return Result<int>.Fail("An invoice already exists for this ticket.");

            var ticketWithItems = await _unitOfWork.Repository<ServiceTicket>()
                .GetAllWithIncludesAsync<ServiceTicket>(
                    t => t.Id == ticketId,
                    t => t,
                    t => t.Items,
                    t => t.Bike);

            var ticket = ticketWithItems.FirstOrDefault();
            if (ticket == null)
                return Result<int>.Fail("Service ticket not found.");

            var totalAmount = ticket.Items.Sum(i => i.Quantity * i.UnitPrice);
            var taxAmount = Math.Round(totalAmount * TaxRate, 2);
            var finalAmount = totalAmount + taxAmount;

            var invoice = new Invoice
            {
                ServiceTicketId = ticketId,
                TotalAmount = totalAmount,
                TaxAmount = taxAmount,
                DiscountAmount = 0,
                FinalAmount = finalAmount,
                Status = InvoiceStatus.Draft,
                CreatedBy = _userContextService.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Invoice>().AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "Invoice", "Create",
                _userContextService.UserId, _userContextService.Email,
                $"Generated invoice for ticket ID {ticketId}",
                entityId: invoice.Id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: null,
                newValues: JsonSerializer.Serialize(new
                {
                    invoice.ServiceTicketId,
                    invoice.TotalAmount,
                    invoice.TaxAmount,
                    invoice.FinalAmount,
                    Status = invoice.Status.ToString()
                }));

            return Result<int>.Ok(invoice.Id, "Invoice generated successfully.");
        }

        public async Task<Result<bool>> IssueAsync(int id)
        {
            var invoice = await _unitOfWork.Repository<Invoice>().GetByIdAsync(id);
            if (invoice == null)
                return Result<bool>.Fail("Invoice not found.");

            if (invoice.Status != InvoiceStatus.Draft)
                return Result<bool>.Fail("Only draft invoices can be issued.");

            var oldValues = JsonSerializer.Serialize(new { Status = invoice.Status.ToString() });

            invoice.Status = InvoiceStatus.Issued;
            invoice.UpdatedBy = _userContextService.UserId;
            invoice.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Invoice>().Update(invoice);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "Invoice", "Issue",
                _userContextService.UserId, _userContextService.Email,
                $"Issued invoice ID {id}",
                entityId: id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: JsonSerializer.Serialize(new { Status = InvoiceStatus.Issued.ToString() }));

            // Notify customer
            var ticket = await _unitOfWork.Repository<ServiceTicket>().GetByIdAsync(invoice.ServiceTicketId);
            if (ticket != null)
            {
                var bike = await _unitOfWork.Repository<CustomerBike>().GetByIdAsync(ticket.BikeId);
                if (bike != null && !string.IsNullOrEmpty(bike.CustomerId))
                {
                    await _notificationService.CreateNotificationAsync(
                        bike.CustomerId,
                        "Invoice Ready",
                        $"Your invoice (#{id}) is ready. Please review and complete payment.",
                        link: $"/Invoice/Detail/{id}");
                }
            }

            return Result<bool>.Ok(true, "Invoice issued successfully.");
        }

        public async Task<Result<bool>> VoidAsync(int id)
        {
            var invoice = await _unitOfWork.Repository<Invoice>().GetByIdAsync(id);
            if (invoice == null)
                return Result<bool>.Fail("Invoice not found.");

            if (invoice.Status == InvoiceStatus.Paid)
                return Result<bool>.Fail("Cannot void a paid invoice.");

            if (invoice.Status == InvoiceStatus.Void)
                return Result<bool>.Fail("Invoice is already voided.");

            var oldValues = JsonSerializer.Serialize(new { Status = invoice.Status.ToString() });

            invoice.Status = InvoiceStatus.Void;
            invoice.UpdatedBy = _userContextService.UserId;
            invoice.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Invoice>().Update(invoice);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "Invoice", "Void",
                _userContextService.UserId, _userContextService.Email,
                $"Voided invoice ID {id}",
                entityId: id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: JsonSerializer.Serialize(new { Status = InvoiceStatus.Void.ToString() }));

            return Result<bool>.Ok(true, "Invoice voided successfully.");
        }

        // --- Private helpers ---

        private async Task<Invoice?> LoadInvoiceWithNavigationsAsync(int id)
        {
            var invoices = await _unitOfWork.Repository<Invoice>()
                .GetAllWithIncludesAsync<Invoice>(
                    i => i.Id == id,
                    i => i,
                    i => i.ServiceTicket,
                    i => i.PromoCode,
                    i => i.PaymentTransactions);

            var invoice = invoices.FirstOrDefault();
            if (invoice == null) return null;

            if (invoice.ServiceTicket != null && invoice.ServiceTicket.Bike == null)
            {
                invoice.ServiceTicket.Bike = await _unitOfWork.Repository<CustomerBike>()
                    .GetByIdAsync(invoice.ServiceTicket.BikeId);
            }

            // Load ticket items with service type and part names
            if (invoice.ServiceTicket != null)
            {
                var items = await _unitOfWork.Repository<ServiceTicketItem>()
                    .Where(i => i.ServiceTicketId == invoice.ServiceTicketId);

                foreach (var item in items)
                {
                    if (item.ServiceTypeId.HasValue && item.ServiceType == null)
                        item.ServiceType = await _unitOfWork.Repository<ServiceType>().GetByIdAsync(item.ServiceTypeId.Value);
                    if (item.PartId.HasValue && item.Part == null)
                        item.Part = await _unitOfWork.Repository<Part>().GetByIdAsync(item.PartId.Value);
                }

                invoice.ServiceTicket.Items = items.ToList();
            }

            foreach (var tx in invoice.PaymentTransactions)
            {
                if (tx.Gateway == null)
                    tx.Gateway = await _unitOfWork.Repository<PaymentGateway>().GetByIdAsync(tx.GatewayId);
            }

            return invoice;
        }
    }
}
