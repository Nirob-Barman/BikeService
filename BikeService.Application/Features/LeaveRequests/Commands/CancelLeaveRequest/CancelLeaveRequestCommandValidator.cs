using FluentValidation;

namespace BikeService.Application.Features.LeaveRequests.Commands.CancelLeaveRequest;

public class CancelLeaveRequestCommandValidator : AbstractValidator<CancelLeaveRequestCommand>
{
    public CancelLeaveRequestCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
