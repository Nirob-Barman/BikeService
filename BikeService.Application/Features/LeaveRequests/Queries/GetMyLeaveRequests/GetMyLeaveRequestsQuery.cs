using BikeService.Application.DTOs.LeaveRequest;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.LeaveRequests.Queries.GetMyLeaveRequests;

public record GetMyLeaveRequestsQuery : IRequest<Result<List<LeaveRequestDto>>>;
