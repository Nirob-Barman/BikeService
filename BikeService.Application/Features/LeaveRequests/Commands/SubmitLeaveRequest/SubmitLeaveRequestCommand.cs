using BikeService.Application.Wrappers;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.LeaveRequests.Commands.SubmitLeaveRequest;

public record SubmitLeaveRequestCommand(
    DateTime FromDate,
    DateTime ToDate,
    LeaveType Type,
    string? Reason) : IRequest<Result<int>>;
