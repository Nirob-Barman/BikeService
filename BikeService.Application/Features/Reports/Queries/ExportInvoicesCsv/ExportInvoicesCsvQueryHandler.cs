using System.Text;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.Reports.Queries.ExportInvoicesCsv;

public class ExportInvoicesCsvQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<ExportInvoicesCsvQuery, string>
{
    public async Task<string> Handle(ExportInvoicesCsvQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var dateTo = filter.DateTo.Date.AddDays(1);

        var invoices = await unitOfWork.Repository<Invoice>()
            .GetAllWithIncludesAsync<Invoice>(
                i => i.Status == InvoiceStatus.Paid
                  && i.CreatedAt >= filter.DateFrom.Date
                  && i.CreatedAt < dateTo,
                i => i,
                i => i.ServiceTicket);

        foreach (var inv in invoices)
        {
            if (inv.ServiceTicket != null && inv.ServiceTicket.Bike == null)
                inv.ServiceTicket.Bike = await unitOfWork.Repository<CustomerBike>().GetByIdAsync(inv.ServiceTicket.BikeId);
        }

        var sb = new StringBuilder();
        sb.AppendLine("Invoice #,Date,Bike,Subtotal,Tax,Discount,Total");

        foreach (var inv in invoices.OrderByDescending(i => i.CreatedAt))
        {
            var bike = inv.ServiceTicket?.Bike != null
                ? $"{inv.ServiceTicket.Bike.Year} {inv.ServiceTicket.Bike.Make} {inv.ServiceTicket.Bike.Model}"
                : "";
            sb.AppendLine($"{inv.Id},{inv.CreatedAt:yyyy-MM-dd},\"{bike}\",{inv.TotalAmount},{inv.TaxAmount},{inv.DiscountAmount},{inv.FinalAmount}");
        }

        return sb.ToString();
    }
}
