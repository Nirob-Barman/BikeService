using BikeService.Application.DTOs.LeaveRequest;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.LeaveRequests.Queries.GetLeaveRequestById;

public record GetLeaveRequestByIdQuery(int Id) : IRequest<Result<LeaveRequestDto>>;
