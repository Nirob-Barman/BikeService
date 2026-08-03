using BikeService.Application.DTOs.Payroll;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Payroll.Queries.GetPayrollRecordById;

public class GetPayrollRecordByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetPayrollRecordByIdQuery, Result<PayrollRecordDto>>
{
    public async Task<Result<PayrollRecordDto>> Handle(GetPayrollRecordByIdQuery request, CancellationToken cancellationToken)
    {
        var records = await unitOfWork.Repository<PayrollRecord>()
            .GetAllWithIncludesAsync(
                predicate: p => p.Id == request.Id,
                selector: p => PayrollRecordMapper.ToDto(p),
                includes: p => p.Mechanic);

        var dto = records.FirstOrDefault();
        if (dto == null)
            return Result<PayrollRecordDto>.Fail("Payroll record not found.");

        return Result<PayrollRecordDto>.Ok(dto);
    }
}
