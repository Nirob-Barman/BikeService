using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.PaymentGateways.Queries.GetDecryptedPaymentGatewayConfig;

public class GetDecryptedPaymentGatewayConfigQueryHandler(
    IUnitOfWork unitOfWork,
    IConfigEncryptor configEncryptor) : IRequestHandler<GetDecryptedPaymentGatewayConfigQuery, Result<string>>
{
    public async Task<Result<string>> Handle(GetDecryptedPaymentGatewayConfigQuery request, CancellationToken cancellationToken)
    {
        var gateway = await unitOfWork.Repository<PaymentGateway>().GetByIdAsync(request.Id);
        if (gateway == null)
            return Result<string>.Fail("Payment gateway not found.");

        try
        {
            var decrypted = configEncryptor.Decrypt(gateway.Config);
            return Result<string>.Ok(decrypted);
        }
        catch
        {
            return Result<string>.Fail("Failed to decrypt gateway configuration.");
        }
    }
}
