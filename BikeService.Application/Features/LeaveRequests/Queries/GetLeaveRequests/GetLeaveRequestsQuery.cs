using BikeService.Application.DTOs.LeaveRequest;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.LeaveRequests.Queries.GetLeaveRequests;

public record GetLeaveRequestsQuery : IRequest<Result<List<LeaveRequestDto>>>;
