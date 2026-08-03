using FluentValidation;

namespace BikeService.Application.Features.LeaveRequests.Commands.RejectLeaveRequest;

public class RejectLeaveRequestCommandValidator : AbstractValidator<RejectLeaveRequestCommand>
{
    public RejectLeaveRequestCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
