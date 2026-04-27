using System.Text.Json;
using BikeService.Application.DTOs.ServiceType;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Interfaces.Services;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;

namespace BikeService.Application.Services
{
    public class ServiceTypeService : IServiceTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;
        private readonly IUserContextService _userContextService;

        public ServiceTypeService(
            IUnitOfWork unitOfWork,
            IAuditLogService auditLogService,
            IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
            _userContextService = userContextService;
        }

        public async Task<Result<List<ServiceTypeDto>>> GetAllAsync()
        {
            var items = await _unitOfWork.Repository<ServiceType>()
                .GetAllAsync<ServiceTypeDto>(e => ServiceTypeMapper.ToDto(e));
            return Result<List<ServiceTypeDto>>.Ok(items.ToList());
        }

        public async Task<Result<ServiceTypeDto>> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.Repository<ServiceType>().GetByIdAsync(id);
            if (entity == null)
                return Result<ServiceTypeDto>.Fail("Service type not found.");
            return Result<ServiceTypeDto>.Ok(ServiceTypeMapper.ToDto(entity));
        }

        public async Task<Result<List<ServiceTypeDto>>> GetActiveAsync()
        {
            var items = await _unitOfWork.Repository<ServiceType>()
                .GetAllAsync<ServiceTypeDto>(e => e.IsActive, e => ServiceTypeMapper.ToDto(e));
            return Result<List<ServiceTypeDto>>.Ok(items.ToList());
        }

        public async Task<Result<int>> CreateAsync(ServiceTypeFormDto dto)
        {
            var duplicate = await _unitOfWork.Repository<ServiceType>()
                .AnyAsync(e => e.Name == dto.Name);
            if (duplicate)
                return Result<int>.FailField("Name", "A service type with this name already exists.");

            var entity = ServiceTypeMapper.ToEntity(dto);
            entity.CreatedBy = _userContextService.UserId;
            entity.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<ServiceType>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "ServiceType", "Create",
                _userContextService.UserId, _userContextService.Email,
                $"Created service type '{entity.Name}'",
                entityId: entity.Id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: null,
                newValues: JsonSerializer.Serialize(new
                {
                    entity.Name,
                    entity.Description,
                    entity.BasePrice,
                    entity.EstimatedHours,
                    entity.IsActive
                }));

            return Result<int>.Ok(entity.Id, "Service type created successfully.");
        }

        public async Task<Result<bool>> UpdateAsync(int id, ServiceTypeFormDto dto)
        {
            var entity = await _unitOfWork.Repository<ServiceType>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Service type not found.");

            var duplicate = await _unitOfWork.Repository<ServiceType>()
                .AnyAsync(e => e.Name == dto.Name && e.Id != id);
            if (duplicate)
                return Result<bool>.FailField("Name", "A service type with this name already exists.");

            var oldValues = JsonSerializer.Serialize(new
            {
                entity.Name,
                entity.Description,
                entity.BasePrice,
                entity.EstimatedHours,
                entity.IsActive
            });

            ServiceTypeMapper.UpdateEntity(entity, dto);
            entity.UpdatedBy = _userContextService.UserId;
            entity.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<ServiceType>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "ServiceType", "Update",
                _userContextService.UserId, _userContextService.Email,
                $"Updated service type '{entity.Name}'",
                entityId: entity.Id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: JsonSerializer.Serialize(new
                {
                    entity.Name,
                    entity.Description,
                    entity.BasePrice,
                    entity.EstimatedHours,
                    entity.IsActive
                }));

            return Result<bool>.Ok(true, "Service type updated successfully.");
        }

        public async Task<Result<bool>> ToggleActiveAsync(int id)
        {
            var entity = await _unitOfWork.Repository<ServiceType>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Service type not found.");

            var oldValues = JsonSerializer.Serialize(new { entity.IsActive });

            entity.IsActive = !entity.IsActive;
            entity.UpdatedBy = _userContextService.UserId;
            entity.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<ServiceType>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "ServiceType", "Toggle",
                _userContextService.UserId, _userContextService.Email,
                $"Toggled service type '{entity.Name}' IsActive to {entity.IsActive}",
                entityId: entity.Id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: JsonSerializer.Serialize(new { entity.IsActive }));

            return Result<bool>.Ok(true, $"Service type is now {(entity.IsActive ? "active" : "inactive")}.");
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.Repository<ServiceType>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Service type not found.");

            var hasItems = await _unitOfWork.Repository<ServiceTicketItem>()
                .AnyAsync(i => i.ServiceTypeId == id);
            if (hasItems)
                return Result<bool>.Fail("Cannot delete this service type because it is referenced by existing service ticket items.");

            var oldValues = JsonSerializer.Serialize(new
            {
                entity.Name,
                entity.Description,
                entity.BasePrice,
                entity.EstimatedHours,
                entity.IsActive
            });

            _unitOfWork.Repository<ServiceType>().Remove(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "ServiceType", "Delete",
                _userContextService.UserId, _userContextService.Email,
                $"Deleted service type '{entity.Name}'",
                entityId: id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: null);

            return Result<bool>.Ok(true, "Service type deleted successfully.");
        }
    }
}
