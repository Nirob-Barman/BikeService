using FluentValidation;

namespace BikeService.Application.Features.LeaveRequests.Commands.SubmitLeaveRequest;

public class SubmitLeaveRequestCommandValidator : AbstractValidator<SubmitLeaveRequestCommand>
{
    public SubmitLeaveRequestCommandValidator()
    {
        RuleFor(x => x.FromDate).NotEmpty();
        RuleFor(x => x.ToDate).NotEmpty();
    }
}
