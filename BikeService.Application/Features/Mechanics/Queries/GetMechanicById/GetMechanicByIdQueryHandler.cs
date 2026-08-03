using BikeService.Application.DTOs.Mechanic;
using BikeService.Application.Interfaces.Identity;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Mechanics.Queries.GetMechanicById;

public class GetMechanicByIdQueryHandler(IUnitOfWork unitOfWork, IUserManager userManager)
    : IRequestHandler<GetMechanicByIdQuery, Result<MechanicDto>>
{
    public async Task<Result<MechanicDto>> Handle(GetMechanicByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<Mechanic>().GetByIdAsync(request.Id);
        if (entity == null)
            return Result<MechanicDto>.Fail("Mechanic not found.");

        var dto = MechanicMapper.ToDto(entity);
        if (!string.IsNullOrEmpty(entity.UserId))
        {
            var user = await userManager.FindByIdAsync(entity.UserId);
            dto.LinkedEmail = user?.Email;
            dto.IsLoginActive = user != null && !user.IsBanned;
        }
        return Result<MechanicDto>.Ok(dto);
    }
}
