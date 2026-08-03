using FluentValidation;

namespace BikeService.Application.Features.LeaveRequests.Commands.ApproveLeaveRequest;

public class ApproveLeaveRequestCommandValidator : AbstractValidator<ApproveLeaveRequestCommand>
{
    public ApproveLeaveRequestCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
