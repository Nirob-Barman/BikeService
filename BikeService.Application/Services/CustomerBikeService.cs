using BikeService.Application.DTOs.CustomerBike;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Interfaces.Services;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;

namespace BikeService.Application.Services
{
    public class CustomerBikeService : ICustomerBikeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContextService;

        public CustomerBikeService(IUnitOfWork unitOfWork, IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
        }

        public async Task<Result<List<CustomerBikeDto>>> GetAllAsync()
        {
            var bikes = await _unitOfWork.Repository<CustomerBike>().GetAllAsync();
            var dtos = bikes.Select(CustomerBikeMapper.ToDto).ToList();
            return Result<List<CustomerBikeDto>>.Ok(dtos);
        }

        public async Task<Result<List<CustomerBikeDto>>> GetMyBikesAsync()
        {
            var userId = _userContextService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Result<List<CustomerBikeDto>>.Fail("User is not authenticated.");

            var bikes = await _unitOfWork.Repository<CustomerBike>().Where(b => b.CustomerId == userId);
            var dtos = bikes.Select(CustomerBikeMapper.ToDto).ToList();
            return Result<List<CustomerBikeDto>>.Ok(dtos);
        }

        public async Task<Result<CustomerBikeDto>> GetByIdAsync(int id)
        {
            var bike = await _unitOfWork.Repository<CustomerBike>().GetByIdAsync(id);
            if (bike is null)
                return Result<CustomerBikeDto>.Fail("Bike not found.");

            return Result<CustomerBikeDto>.Ok(CustomerBikeMapper.ToDto(bike));
        }

        public async Task<Result<int>> CreateAsync(CustomerBikeFormDto dto)
        {
            var userId = _userContextService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Result<int>.Fail("User is not authenticated.");

            var bike = CustomerBikeMapper.ToEntity(dto);
            bike.CustomerId = userId;
            bike.CreatedBy = userId;

            await _unitOfWork.Repository<CustomerBike>().AddAsync(bike);
            await _unitOfWork.SaveChangesAsync();

            return Result<int>.Ok(bike.Id, "Bike registered successfully.");
        }

        public async Task<Result<bool>> UpdateAsync(int id, CustomerBikeFormDto dto)
        {
            var bike = await _unitOfWork.Repository<CustomerBike>().GetByIdAsync(id);
            if (bike is null)
                return Result<bool>.Fail("Bike not found.");

            var userId = _userContextService.UserId;
            if (!_userContextService.IsInRole("Admin") && bike.CustomerId != userId)
                return Result<bool>.Fail("You do not have permission to update this bike.");

            CustomerBikeMapper.UpdateEntity(bike, dto);
            bike.UpdatedAt = DateTime.UtcNow;
            bike.UpdatedBy = userId;

            _unitOfWork.Repository<CustomerBike>().Update(bike);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Ok(true, "Bike updated successfully.");
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var bike = await _unitOfWork.Repository<CustomerBike>().GetByIdAsync(id);
            if (bike is null)
                return Result<bool>.Fail("Bike not found.");

            var userId = _userContextService.UserId;
            if (!_userContextService.IsInRole("Admin") && bike.CustomerId != userId)
                return Result<bool>.Fail("You do not have permission to delete this bike.");

            var hasActiveTickets = await _unitOfWork.Repository<ServiceTicket>()
                .AnyAsync(t => t.BikeId == id &&
                               t.Status != Domain.Enums.ServiceTicketStatus.Delivered &&
                               t.Status != Domain.Enums.ServiceTicketStatus.Cancelled);

            if (hasActiveTickets)
                return Result<bool>.Fail("Cannot delete bike because it has active service tickets.");

            _unitOfWork.Repository<CustomerBike>().Remove(bike);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Ok(true, "Bike deleted successfully.");
        }
    }
}
