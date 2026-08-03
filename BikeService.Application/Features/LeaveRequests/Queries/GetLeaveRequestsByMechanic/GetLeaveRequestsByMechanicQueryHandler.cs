using BikeService.Application.DTOs.LeaveRequest;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.LeaveRequests.Queries.GetLeaveRequestsByMechanic;

public class GetLeaveRequestsByMechanicQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetLeaveRequestsByMechanicQuery, Result<List<LeaveRequestDto>>>
{
    public async Task<Result<List<LeaveRequestDto>>> Handle(GetLeaveRequestsByMechanicQuery request, CancellationToken cancellationToken)
    {
        var requests = await unitOfWork.Repository<LeaveRequest>()
            .GetAllWithIncludesAsync(
                predicate: l => l.MechanicId == request.MechanicId,
                selector: l => LeaveRequestMapper.ToDto(l),
                includes: l => l.Mechanic);

        return Result<List<LeaveRequestDto>>.Ok(requests.OrderByDescending(r => r.CreatedAt).ToList());
    }
}
