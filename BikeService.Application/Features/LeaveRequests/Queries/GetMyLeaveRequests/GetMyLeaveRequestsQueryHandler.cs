using BikeService.Application.DTOs.LeaveRequest;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.LeaveRequests.Queries.GetMyLeaveRequests;

public class GetMyLeaveRequestsQueryHandler(
    IUnitOfWork unitOfWork,
    IUserContextService userContextService) : IRequestHandler<GetMyLeaveRequestsQuery, Result<List<LeaveRequestDto>>>
{
    public async Task<Result<List<LeaveRequestDto>>> Handle(GetMyLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        var mechanic = await unitOfWork.Repository<Mechanic>()
            .FirstOrDefaultAsync(m => m.UserId == userContextService.UserId);

        if (mechanic == null)
            return Result<List<LeaveRequestDto>>.Fail("Mechanic profile not found.");

        var requests = await unitOfWork.Repository<LeaveRequest>()
            .GetAllWithIncludesAsync(
                predicate: l => l.MechanicId == mechanic.Id,
                selector: l => LeaveRequestMapper.ToDto(l),
                includes: l => l.Mechanic);

        return Result<List<LeaveRequestDto>>.Ok(requests.OrderByDescending(r => r.CreatedAt).ToList());
    }
}
