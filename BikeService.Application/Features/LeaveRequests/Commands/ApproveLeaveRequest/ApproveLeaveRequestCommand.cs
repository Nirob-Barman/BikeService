using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.LeaveRequests.Commands.ApproveLeaveRequest;

public record ApproveLeaveRequestCommand(int Id, string? AdminNotes) : IRequest<Result<bool>>;
