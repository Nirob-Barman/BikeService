using System.Text.Json;
using BikeService.Application.Features.PaymentGateways.Queries.GetDecryptedPaymentGatewayConfig;
using BikeService.Application.Features.PromoCodes.Queries.ValidatePromoCode;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.Payments.Commands.InitiatePayment;

public class InitiatePaymentCommandHandler(
    IUnitOfWork unitOfWork,
    IPaymentProcessorFactory processorFactory,
    IUserContextService userContextService,
    IMediator mediator) : IRequestHandler<InitiatePaymentCommand, Result<string>>
{
    public async Task<Result<string>> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
    {
        var userId = userContextService.UserId;

        var invoices = await unitOfWork.Repository<Invoice>()
            .GetAllWithIncludesAsync<Invoice>(
                i => i.Id == request.InvoiceId,
                i => i,
                i => i.ServiceTicket);

        var invoice = invoices.FirstOrDefault();
        if (invoice == null)
            return Result<string>.Fail("Invoice not found.");

        if (invoice.ServiceTicket?.Bike == null)
            invoice.ServiceTicket!.Bike = await unitOfWork.Repository<CustomerBike>()
                .GetByIdAsync(invoice.ServiceTicket.BikeId);

        if (invoice.ServiceTicket?.Bike?.CustomerId != userId)
            return Result<string>.Fail("Access denied.");

        if (invoice.Status == InvoiceStatus.Paid)
            return Result<string>.Fail("Invoice is already paid.");
        if (invoice.Status != InvoiceStatus.Issued)
            return Result<string>.Fail("Invoice must be in Issued status before payment.");

        var gateway = await unitOfWork.Repository<PaymentGateway>().GetByIdAsync(request.GatewayId);
        if (gateway == null || !gateway.IsActive)
            return Result<string>.Fail("Payment gateway not available.");

        var processor = processorFactory.GetProcessor(gateway.Slug);
        if (processor == null)
            return Result<string>.Fail($"Payment processor '{gateway.Slug}' is not configured.");

        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            var promoResult = await mediator.Send(new ValidatePromoCodeQuery(request.PromoCode), cancellationToken);
            if (promoResult.Success && promoResult.Data != null)
            {
                var discountAmt = Math.Round(invoice.TotalAmount * (promoResult.Data.DiscountPercent / 100m), 2);
                invoice.DiscountAmount = discountAmt;
                invoice.PromoCodeId = promoResult.Data.Id;
                invoice.FinalAmount = Math.Max(0, invoice.TotalAmount + invoice.TaxAmount - discountAmt);
                invoice.UpdatedAt = DateTime.UtcNow;
                unitOfWork.Repository<Invoice>().Update(invoice);
            }
        }

        var tx = new PaymentTransaction
        {
            InvoiceId = invoice.Id,
            GatewayId = request.GatewayId,
            Amount = invoice.FinalAmount,
            Status = PaymentTransactionStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        await unitOfWork.Repository<PaymentTransaction>().AddAsync(tx);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var configResult = await mediator.Send(new GetDecryptedPaymentGatewayConfigQuery(request.GatewayId), cancellationToken);
        if (!configResult.Success)
            return Result<string>.Fail("Failed to load gateway configuration.");

        var config = JsonSerializer.Deserialize<Dictionary<string, string>>(configResult.Data!)
                     ?? new Dictionary<string, string>();

        var baseUrl = userContextService.GetBaseUrl();
        var successUrl = $"{baseUrl}/Payment/Success?txId={tx.Id}&gateway={gateway.Slug}";
        var cancelUrl = $"{baseUrl}/Payment/Cancel?txId={tx.Id}";

        var initResult = await processor.InitiateAsync(config, invoice.FinalAmount, tx.Id, successUrl, cancelUrl);
        if (!initResult.Success)
        {
            tx.Status = PaymentTransactionStatus.Failed;
            unitOfWork.Repository<PaymentTransaction>().Update(tx);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<string>.Fail(initResult.Error ?? "Payment initiation failed.");
        }

        return Result<string>.Ok(initResult.RedirectUrl!);
    }
}
