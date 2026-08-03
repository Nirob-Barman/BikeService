using BikeService.Application.DTOs.Invoice;
using BikeService.Application.Features.Invoices.Common;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Invoices.Queries.GetMyInvoiceById;

public class GetMyInvoiceByIdQueryHandler(IUnitOfWork unitOfWork, IUserContextService userContextService)
    : IRequestHandler<GetMyInvoiceByIdQuery, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(GetMyInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = userContextService.UserId;
        if (string.IsNullOrEmpty(userId))
            return Result<InvoiceDto>.Fail("User is not authenticated.");

        var invoice = await InvoiceNavigationLoader.LoadWithNavigationsAsync(unitOfWork, request.Id);
        if (invoice == null)
            return Result<InvoiceDto>.Fail("Invoice not found.");

        if (invoice.ServiceTicket?.Bike?.CustomerId != userId)
            return Result<InvoiceDto>.Fail("Access denied.");

        return Result<InvoiceDto>.Ok(InvoiceMapper.ToDto(invoice));
    }
}
