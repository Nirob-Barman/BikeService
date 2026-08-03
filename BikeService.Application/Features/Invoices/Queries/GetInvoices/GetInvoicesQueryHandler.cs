using BikeService.Application.DTOs.Invoice;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Invoices.Queries.GetInvoices;

public class GetInvoicesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetInvoicesQuery, Result<List<InvoiceDto>>>
{
    public async Task<Result<List<InvoiceDto>>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        var invoices = await unitOfWork.Repository<Invoice>()
            .GetAllWithIncludesAsync<Invoice>(
                i => i,
                i => i.ServiceTicket,
                i => i.PromoCode,
                i => i.PaymentTransactions);

        foreach (var inv in invoices)
        {
            if (inv.ServiceTicket != null && inv.ServiceTicket.Bike == null)
            {
                inv.ServiceTicket.Bike = await unitOfWork.Repository<CustomerBike>()
                    .GetByIdAsync(inv.ServiceTicket.BikeId);
            }
        }

        var filter = request.Filter;
        if (filter != null)
        {
            if (filter.Status.HasValue)
                invoices = invoices.Where(i => i.Status == filter.Status.Value);
            if (filter.DateFrom.HasValue)
                invoices = invoices.Where(i => i.CreatedAt >= filter.DateFrom.Value);
            if (filter.DateTo.HasValue)
                invoices = invoices.Where(i => i.CreatedAt <= filter.DateTo.Value);
        }

        foreach (var inv in invoices)
        {
            foreach (var tx in inv.PaymentTransactions)
            {
                if (tx.Gateway == null)
                    tx.Gateway = await unitOfWork.Repository<PaymentGateway>().GetByIdAsync(tx.GatewayId);
            }
        }

        var dtos = invoices.Select(InvoiceMapper.ToDto).ToList();
        return Result<List<InvoiceDto>>.Ok(dtos);
    }
}
