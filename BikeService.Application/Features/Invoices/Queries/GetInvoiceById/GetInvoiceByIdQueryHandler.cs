using BikeService.Application.DTOs.Invoice;
using BikeService.Application.Features.Invoices.Common;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Invoices.Queries.GetInvoiceById;

public class GetInvoiceByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await InvoiceNavigationLoader.LoadWithNavigationsAsync(unitOfWork, request.Id);
        if (invoice == null)
            return Result<InvoiceDto>.Fail("Invoice not found.");

        return Result<InvoiceDto>.Ok(InvoiceMapper.ToDto(invoice));
    }
}
