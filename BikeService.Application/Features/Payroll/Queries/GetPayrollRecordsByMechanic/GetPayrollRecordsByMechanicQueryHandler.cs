using BikeService.Application.DTOs.Payroll;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Payroll.Queries.GetPayrollRecordsByMechanic;

public class GetPayrollRecordsByMechanicQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetPayrollRecordsByMechanicQuery, Result<List<PayrollRecordDto>>>
{
    public async Task<Result<List<PayrollRecordDto>>> Handle(GetPayrollRecordsByMechanicQuery request, CancellationToken cancellationToken)
    {
        var records = await unitOfWork.Repository<PayrollRecord>()
            .GetAllWithIncludesAsync(
                predicate: p => p.MechanicId == request.MechanicId,
                selector: p => PayrollRecordMapper.ToDto(p),
                includes: p => p.Mechanic);

        return Result<List<PayrollRecordDto>>.Ok(
            records.OrderByDescending(r => r.Year).ThenByDescending(r => r.Month).ToList());
    }
}
