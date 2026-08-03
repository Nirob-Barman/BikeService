using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.LeaveRequests.Commands.RejectLeaveRequest;

public record RejectLeaveRequestCommand(int Id, string? AdminNotes) : IRequest<Result<bool>>;
