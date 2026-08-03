using BikeService.Application.DTOs.LeaveRequest;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.LeaveRequests.Queries.GetLeaveRequestsByMechanic;

public record GetLeaveRequestsByMechanicQuery(int MechanicId) : IRequest<Result<List<LeaveRequestDto>>>;
