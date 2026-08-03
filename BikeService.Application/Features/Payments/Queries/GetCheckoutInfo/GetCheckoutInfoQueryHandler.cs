using BikeService.Application.DTOs.Payment;
using BikeService.Application.Features.PaymentGateways.Queries.GetPaymentGateways;
using BikeService.Application.Features.PromoCodes.Queries.ValidatePromoCode;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.Payments.Queries.GetCheckoutInfo;

public class GetCheckoutInfoQueryHandler(
    IUnitOfWork unitOfWork,
    IUserContextService userContextService,
    IMediator mediator) : IRequestHandler<GetCheckoutInfoQuery, Result<CheckoutInfoDto>>
{
    public async Task<Result<CheckoutInfoDto>> Handle(GetCheckoutInfoQuery request, CancellationToken cancellationToken)
    {
        var userId = userContextService.UserId;

        var invoices = await unitOfWork.Repository<Invoice>()
            .GetAllWithIncludesAsync<Invoice>(
                i => i.Id == request.InvoiceId,
                i => i,
                i => i.ServiceTicket,
                i => i.PromoCode);

        var invoice = invoices.FirstOrDefault();
        if (invoice == null)
            return Result<CheckoutInfoDto>.Fail("Invoice not found.");

        if (invoice.ServiceTicket?.Bike == null)
            invoice.ServiceTicket!.Bike = await unitOfWork.Repository<CustomerBike>()
                .GetByIdAsync(invoice.ServiceTicket.BikeId);

        if (invoice.ServiceTicket?.Bike?.CustomerId != userId)
            return Result<CheckoutInfoDto>.Fail("Access denied.");

        if (invoice.Status == InvoiceStatus.Paid)
            return Result<CheckoutInfoDto>.Fail("Invoice is already paid.");
        if (invoice.Status == InvoiceStatus.Void)
            return Result<CheckoutInfoDto>.Fail("Invoice is void.");

        var gatewaysResult = await mediator.Send(new GetPaymentGatewaysQuery(), cancellationToken);
        var gateways = (gatewaysResult.Data ?? new()).Where(g => g.IsActive).ToList();

        decimal discountAmount = 0;
        decimal discountPercent = 0;
        string? appliedCode = null;

        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            var promoResult = await mediator.Send(new ValidatePromoCodeQuery(request.PromoCode), cancellationToken);
            if (promoResult.Success && promoResult.Data != null)
            {
                discountPercent = promoResult.Data.DiscountPercent;
                discountAmount = Math.Round(invoice.TotalAmount * (discountPercent / 100m), 2);
                appliedCode = promoResult.Data.Code;
            }
        }

        var bike = invoice.ServiceTicket?.Bike;
        var bikeSummary = bike == null ? "Bike Service" : $"{bike.Year} {bike.Make} {bike.Model}";
        var finalAmount = Math.Max(0, invoice.TotalAmount + invoice.TaxAmount - discountAmount);

        return Result<CheckoutInfoDto>.Ok(new CheckoutInfoDto
        {
            InvoiceId = invoice.Id,
            BikeSummary = bikeSummary,
            TotalAmount = invoice.TotalAmount,
            TaxAmount = invoice.TaxAmount,
            DiscountAmount = discountAmount,
            FinalAmount = finalAmount,
            AppliedPromoCode = appliedCode,
            PromoDiscountPercent = discountPercent,
            Gateways = gateways
        });
    }
}
