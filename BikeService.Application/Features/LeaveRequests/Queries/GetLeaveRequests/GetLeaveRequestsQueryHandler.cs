using BikeService.Application.DTOs.LeaveRequest;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.LeaveRequests.Queries.GetLeaveRequests;

public class GetLeaveRequestsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetLeaveRequestsQuery, Result<List<LeaveRequestDto>>>
{
    public async Task<Result<List<LeaveRequestDto>>> Handle(GetLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        var requests = await unitOfWork.Repository<LeaveRequest>()
            .GetAllWithIncludesAsync(
                selector: l => LeaveRequestMapper.ToDto(l),
                includes: l => l.Mechanic);

        return Result<List<LeaveRequestDto>>.Ok(requests.OrderByDescending(r => r.CreatedAt).ToList());
    }
}
