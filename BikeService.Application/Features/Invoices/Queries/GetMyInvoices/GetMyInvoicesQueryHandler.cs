using BikeService.Application.DTOs.Invoice;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Invoices.Queries.GetMyInvoices;

public class GetMyInvoicesQueryHandler(IUnitOfWork unitOfWork, IUserContextService userContextService)
    : IRequestHandler<GetMyInvoicesQuery, Result<List<InvoiceDto>>>
{
    public async Task<Result<List<InvoiceDto>>> Handle(GetMyInvoicesQuery request, CancellationToken cancellationToken)
    {
        var userId = userContextService.UserId;
        if (string.IsNullOrEmpty(userId))
            return Result<List<InvoiceDto>>.Fail("User is not authenticated.");

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

        var filtered = invoices.Where(i => i.ServiceTicket?.Bike?.CustomerId == userId);

        foreach (var inv in filtered)
        {
            foreach (var tx in inv.PaymentTransactions)
            {
                if (tx.Gateway == null)
                    tx.Gateway = await unitOfWork.Repository<PaymentGateway>().GetByIdAsync(tx.GatewayId);
            }
        }

        var dtos = filtered.Select(InvoiceMapper.ToDto).ToList();
        return Result<List<InvoiceDto>>.Ok(dtos);
    }
}
