using BikeService.Application.Interfaces.Persistence;
using BikeService.Domain.Entities;

namespace BikeService.Application.Features.Invoices.Common;

public static class InvoiceNavigationLoader
{
    public static async Task<Invoice?> LoadWithNavigationsAsync(IUnitOfWork unitOfWork, int id)
    {
        var invoices = await unitOfWork.Repository<Invoice>()
            .GetAllWithIncludesAsync<Invoice>(
                i => i.Id == id,
                i => i,
                i => i.ServiceTicket,
                i => i.PromoCode,
                i => i.PaymentTransactions);

        var invoice = invoices.FirstOrDefault();
        if (invoice == null) return null;

        if (invoice.ServiceTicket != null && invoice.ServiceTicket.Bike == null)
        {
            invoice.ServiceTicket.Bike = await unitOfWork.Repository<CustomerBike>()
                .GetByIdAsync(invoice.ServiceTicket.BikeId);
        }

        if (invoice.ServiceTicket != null)
        {
            var items = await unitOfWork.Repository<ServiceTicketItem>()
                .Where(i => i.ServiceTicketId == invoice.ServiceTicketId);

            foreach (var item in items)
            {
                if (item.ServiceTypeId.HasValue && item.ServiceType == null)
                    item.ServiceType = await unitOfWork.Repository<ServiceType>().GetByIdAsync(item.ServiceTypeId.Value);
                if (item.PartId.HasValue && item.Part == null)
                    item.Part = await unitOfWork.Repository<Part>().GetByIdAsync(item.PartId.Value);
            }

            invoice.ServiceTicket.Items = items.ToList();
        }

        foreach (var tx in invoice.PaymentTransactions)
        {
            if (tx.Gateway == null)
                tx.Gateway = await unitOfWork.Repository<PaymentGateway>().GetByIdAsync(tx.GatewayId);
        }

        return invoice;
    }
}
