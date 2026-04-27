using BikeService.Application.DTOs.LeaveRequest;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Interfaces.Services;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;

namespace BikeService.Application.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;
        private readonly IUserContextService _userContextService;

        public LeaveRequestService(
            IUnitOfWork unitOfWork,
            IAuditLogService auditLogService,
            IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
            _userContextService = userContextService;
        }

        public async Task<Result<List<LeaveRequestDto>>> GetAllAsync()
        {
            var requests = await _unitOfWork.Repository<LeaveRequest>()
                .GetAllWithIncludesAsync(
                    selector: l => LeaveRequestMapper.ToDto(l),
                    includes: l => l.Mechanic);

            return Result<List<LeaveRequestDto>>.Ok(requests.OrderByDescending(r => r.CreatedAt).ToList());
        }

        public async Task<Result<List<LeaveRequestDto>>> GetByMechanicAsync(int mechanicId)
        {
            var requests = await _unitOfWork.Repository<LeaveRequest>()
                .GetAllWithIncludesAsync(
                    predicate: l => l.MechanicId == mechanicId,
                    selector: l => LeaveRequestMapper.ToDto(l),
                    includes: l => l.Mechanic);

            return Result<List<LeaveRequestDto>>.Ok(requests.OrderByDescending(r => r.CreatedAt).ToList());
        }

        public async Task<Result<List<LeaveRequestDto>>> GetMyLeaveRequestsAsync()
        {
            var mechanic = await _unitOfWork.Repository<Mechanic>()
                .FirstOrDefaultAsync(m => m.UserId == _userContextService.UserId);

            if (mechanic == null)
                return Result<List<LeaveRequestDto>>.Fail("Mechanic profile not found.");

            return await GetByMechanicAsync(mechanic.Id);
        }

        public async Task<Result<LeaveRequestDto>> GetByIdAsync(int id)
        {
            var requests = await _unitOfWork.Repository<LeaveRequest>()
                .GetAllWithIncludesAsync(
                    predicate: l => l.Id == id,
                    selector: l => LeaveRequestMapper.ToDto(l),
                    includes: l => l.Mechanic);

            var dto = requests.FirstOrDefault();
            if (dto == null)
                return Result<LeaveRequestDto>.Fail("Leave request not found.");

            return Result<LeaveRequestDto>.Ok(dto);
        }

        public async Task<Result<int>> SubmitAsync(LeaveRequestFormDto dto)
        {
            var mechanic = await _unitOfWork.Repository<Mechanic>()
                .FirstOrDefaultAsync(m => m.UserId == _userContextService.UserId);

            if (mechanic == null)
                return Result<int>.Fail("Mechanic profile not found.");

            var mechanicId = mechanic.Id;

            if (dto.FromDate.Date < DateTime.UtcNow.Date)
                return Result<int>.FailField("FromDate", "Start date cannot be in the past.");

            if (dto.ToDate < dto.FromDate)
                return Result<int>.FailField("ToDate", "End date must be on or after the start date.");

            var hasOverlap = await _unitOfWork.Repository<LeaveRequest>().AnyAsync(l =>
                l.MechanicId == mechanicId &&
                (l.Status == LeaveRequestStatus.Pending || l.Status == LeaveRequestStatus.Approved) &&
                l.FromDate <= dto.ToDate && l.ToDate >= dto.FromDate);

            if (hasOverlap)
                return Result<int>.Fail("A pending or approved leave request already exists for the selected dates.");

            var entity = new LeaveRequest
            {
                MechanicId = mechanicId,
                FromDate   = dto.FromDate.Date,
                ToDate     = dto.ToDate.Date,
                Type       = dto.Type,
                Reason     = dto.Reason?.Trim(),
                Status     = LeaveRequestStatus.Pending,
                CreatedBy  = _userContextService.UserId,
                CreatedAt  = DateTime.UtcNow,
            };

            await _unitOfWork.Repository<LeaveRequest>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync("LeaveRequest", "Create",
                _userContextService.UserId, _userContextService.Email,
                $"Leave request submitted ({dto.Type}, {dto.FromDate:d} – {dto.ToDate:d})",
                entityId: entity.Id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                newValues: System.Text.Json.JsonSerializer.Serialize(new
                {
                    entity.MechanicId, entity.FromDate, entity.ToDate,
                    Type = entity.Type.ToString(), entity.Reason
                }));

            return Result<int>.Ok(entity.Id, "Leave request submitted.");
        }

        public async Task<Result<bool>> ApproveAsync(int id, string? adminNotes)
        {
            var entity = await _unitOfWork.Repository<LeaveRequest>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Leave request not found.");

            if (entity.Status != LeaveRequestStatus.Pending)
                return Result<bool>.Fail("Only pending requests can be approved.");

            var oldStatus = entity.Status.ToString();
            entity.Status     = LeaveRequestStatus.Approved;
            entity.AdminNotes = adminNotes?.Trim();
            entity.UpdatedBy  = _userContextService.UserId;
            entity.UpdatedAt  = DateTime.UtcNow;

            _unitOfWork.Repository<LeaveRequest>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync("LeaveRequest", "Approve",
                _userContextService.UserId, _userContextService.Email,
                $"Leave request #{id} approved",
                entityId: id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: System.Text.Json.JsonSerializer.Serialize(new { Status = oldStatus }),
                newValues: System.Text.Json.JsonSerializer.Serialize(new { Status = "Approved", AdminNotes = adminNotes }));

            return Result<bool>.Ok(true, "Leave request approved.");
        }

        public async Task<Result<bool>> RejectAsync(int id, string? adminNotes)
        {
            var entity = await _unitOfWork.Repository<LeaveRequest>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Leave request not found.");

            if (entity.Status != LeaveRequestStatus.Pending)
                return Result<bool>.Fail("Only pending requests can be rejected.");

            var oldStatus = entity.Status.ToString();
            entity.Status     = LeaveRequestStatus.Rejected;
            entity.AdminNotes = adminNotes?.Trim();
            entity.UpdatedBy  = _userContextService.UserId;
            entity.UpdatedAt  = DateTime.UtcNow;

            _unitOfWork.Repository<LeaveRequest>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync("LeaveRequest", "Reject",
                _userContextService.UserId, _userContextService.Email,
                $"Leave request #{id} rejected",
                entityId: id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: System.Text.Json.JsonSerializer.Serialize(new { Status = oldStatus }),
                newValues: System.Text.Json.JsonSerializer.Serialize(new { Status = "Rejected", AdminNotes = adminNotes }));

            return Result<bool>.Ok(true, "Leave request rejected.");
        }

        public async Task<Result<bool>> CancelAsync(int id)
        {
            var entity = await _unitOfWork.Repository<LeaveRequest>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Leave request not found.");

            if (entity.Status != LeaveRequestStatus.Pending)
                return Result<bool>.Fail("Only pending requests can be cancelled.");

            entity.Status    = LeaveRequestStatus.Cancelled;
            entity.UpdatedBy = _userContextService.UserId;
            entity.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<LeaveRequest>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync("LeaveRequest", "Cancel",
                _userContextService.UserId, _userContextService.Email,
                $"Leave request #{id} cancelled",
                entityId: id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: System.Text.Json.JsonSerializer.Serialize(new { Status = "Pending" }),
                newValues: System.Text.Json.JsonSerializer.Serialize(new { Status = "Cancelled" }));

            return Result<bool>.Ok(true, "Leave request cancelled.");
        }
    }
}
