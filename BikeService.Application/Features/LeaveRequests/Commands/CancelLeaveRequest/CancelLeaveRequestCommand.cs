using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.LeaveRequests.Commands.CancelLeaveRequest;

public record CancelLeaveRequestCommand(int Id) : IRequest<Result<bool>>;
