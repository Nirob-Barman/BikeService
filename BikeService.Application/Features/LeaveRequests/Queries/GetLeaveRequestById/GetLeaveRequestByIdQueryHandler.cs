using BikeService.Application.DTOs.LeaveRequest;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.LeaveRequests.Queries.GetLeaveRequestById;

public class GetLeaveRequestByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetLeaveRequestByIdQuery, Result<LeaveRequestDto>>
{
    public async Task<Result<LeaveRequestDto>> Handle(GetLeaveRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var requests = await unitOfWork.Repository<LeaveRequest>()
            .GetAllWithIncludesAsync(
                predicate: l => l.Id == request.Id,
                selector: l => LeaveRequestMapper.ToDto(l),
                includes: l => l.Mechanic);

        var dto = requests.FirstOrDefault();
        if (dto == null)
            return Result<LeaveRequestDto>.Fail("Leave request not found.");

        return Result<LeaveRequestDto>.Ok(dto);
    }
}
