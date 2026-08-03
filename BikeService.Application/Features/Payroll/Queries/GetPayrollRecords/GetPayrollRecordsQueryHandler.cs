using BikeService.Application.DTOs.Payroll;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Payroll.Queries.GetPayrollRecords;

public class GetPayrollRecordsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetPayrollRecordsQuery, Result<List<PayrollRecordDto>>>
{
    public async Task<Result<List<PayrollRecordDto>>> Handle(GetPayrollRecordsQuery request, CancellationToken cancellationToken)
    {
        var records = await unitOfWork.Repository<PayrollRecord>()
            .GetAllWithIncludesAsync(
                predicate: p => request.Year == null || p.Year == request.Year,
                selector: p => PayrollRecordMapper.ToDto(p),
                includes: p => p.Mechanic);

        return Result<List<PayrollRecordDto>>.Ok(
            records.OrderByDescending(r => r.Year).ThenByDescending(r => r.Month).ToList());
    }
}
