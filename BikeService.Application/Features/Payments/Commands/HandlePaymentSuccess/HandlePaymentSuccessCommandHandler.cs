using System.Text.Json;
using BikeService.Application.Features.PaymentGateways.Queries.GetDecryptedPaymentGatewayConfig;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.Payments.Commands.HandlePaymentSuccess;

public class HandlePaymentSuccessCommandHandler(
    IUnitOfWork unitOfWork,
    IPaymentProcessorFactory processorFactory,
    INotificationService notificationService,
    IMediator mediator) : IRequestHandler<HandlePaymentSuccessCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(HandlePaymentSuccessCommand request, CancellationToken cancellationToken)
    {
        var transactions = await unitOfWork.Repository<PaymentTransaction>()
            .GetAllWithIncludesAsync<PaymentTransaction>(
                t => t.Id == request.TxId,
                t => t,
                t => t.Invoice,
                t => t.Gateway);

        var tx = transactions.FirstOrDefault();
        if (tx == null)
            return Result<bool>.Fail("Transaction not found.");

        if (tx.Status == PaymentTransactionStatus.Success)
            return Result<bool>.Ok(true, "Payment already processed.");

        var processor = processorFactory.GetProcessor(tx.Gateway.Slug);
        if (processor == null)
            return Result<bool>.Fail("Processor not found.");

        var configResult = await mediator.Send(new GetDecryptedPaymentGatewayConfigQuery(tx.GatewayId), cancellationToken);
        if (!configResult.Success)
            return Result<bool>.Fail("Failed to load gateway configuration.");

        var config = JsonSerializer.Deserialize<Dictionary<string, string>>(configResult.Data!)
                     ?? new Dictionary<string, string>();

        var verified = await processor.VerifyAsync(config, request.CallbackParams);
        if (!verified)
        {
            tx.Status = PaymentTransactionStatus.Failed;
            tx.UpdatedAt = DateTime.UtcNow;
            unitOfWork.Repository<PaymentTransaction>().Update(tx);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<bool>.Fail("Payment verification failed.");
        }

        tx.Status = PaymentTransactionStatus.Success;
        tx.UpdatedAt = DateTime.UtcNow;
        unitOfWork.Repository<PaymentTransaction>().Update(tx);

        var invoice = await unitOfWork.Repository<Invoice>().GetByIdAsync(tx.InvoiceId);
        if (invoice != null)
        {
            invoice.Status = InvoiceStatus.Paid;
            invoice.UpdatedAt = DateTime.UtcNow;
            unitOfWork.Repository<Invoice>().Update(invoice);

            if (invoice.PromoCodeId.HasValue)
            {
                var promo = await unitOfWork.Repository<PromoCode>().GetByIdAsync(invoice.PromoCodeId.Value);
                if (promo != null)
                {
                    promo.UsageCount++;
                    promo.UpdatedAt = DateTime.UtcNow;
                    unitOfWork.Repository<PromoCode>().Update(promo);
                }
            }

            var ticket = await unitOfWork.Repository<ServiceTicket>().GetByIdAsync(invoice.ServiceTicketId);
            if (ticket != null && ticket.Status == ServiceTicketStatus.ReadyForPickup)
            {
                ticket.Status = ServiceTicketStatus.Delivered;
                ticket.UpdatedAt = DateTime.UtcNow;
                unitOfWork.Repository<ServiceTicket>().Update(ticket);

                var bike = await unitOfWork.Repository<CustomerBike>().GetByIdAsync(ticket.BikeId);
                if (bike != null)
                {
                    await notificationService.CreateNotificationAsync(
                        bike.CustomerId,
                        "Payment Confirmed",
                        "Your payment has been confirmed. Thank you for using BikeService!",
                        $"/Invoice/Detail/{invoice.Id}");
                }
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true, "Payment processed successfully.");
    }
}
