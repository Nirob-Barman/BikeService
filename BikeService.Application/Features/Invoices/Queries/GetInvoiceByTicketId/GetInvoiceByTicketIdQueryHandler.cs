using BikeService.Application.DTOs.Invoice;
using BikeService.Application.Features.Invoices.Common;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Invoices.Queries.GetInvoiceByTicketId;

public class GetInvoiceByTicketIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetInvoiceByTicketIdQuery, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByTicketIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await unitOfWork.Repository<Invoice>()
            .FirstOrDefaultAsync(i => i.ServiceTicketId == request.TicketId);

        if (invoice == null)
            return Result<InvoiceDto>.Fail("Invoice not found for this ticket.");

        var loaded = await InvoiceNavigationLoader.LoadWithNavigationsAsync(unitOfWork, invoice.Id);
        if (loaded == null)
            return Result<InvoiceDto>.Fail("Invoice not found.");

        return Result<InvoiceDto>.Ok(InvoiceMapper.ToDto(loaded));
    }
}
