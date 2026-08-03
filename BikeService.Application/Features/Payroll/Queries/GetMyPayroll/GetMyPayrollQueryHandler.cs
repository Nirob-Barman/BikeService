using BikeService.Application.DTOs.Payroll;
using BikeService.Application.Features.Payroll.Queries.GetPayrollRecordsByMechanic;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Payroll.Queries.GetMyPayroll;

public class GetMyPayrollQueryHandler(
    IUnitOfWork unitOfWork,
    IUserContextService userContextService,
    IMediator mediator) : IRequestHandler<GetMyPayrollQuery, Result<List<PayrollRecordDto>>>
{
    public async Task<Result<List<PayrollRecordDto>>> Handle(GetMyPayrollQuery request, CancellationToken cancellationToken)
    {
        var mechanic = await unitOfWork.Repository<Mechanic>()
            .FirstOrDefaultAsync(m => m.UserId == userContextService.UserId);

        if (mechanic == null)
            return Result<List<PayrollRecordDto>>.Fail("Mechanic profile not found.");

        return await mediator.Send(new GetPayrollRecordsByMechanicQuery(mechanic.Id), cancellationToken);
    }
}
