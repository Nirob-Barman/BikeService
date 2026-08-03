using BikeService.Application.DTOs.PaymentGateway;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.PaymentGateways.Queries.GetPaymentGatewayById;

public class GetPaymentGatewayByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetPaymentGatewayByIdQuery, Result<PaymentGatewayDto>>
{
    public async Task<Result<PaymentGatewayDto>> Handle(GetPaymentGatewayByIdQuery request, CancellationToken cancellationToken)
    {
        var gateways = await unitOfWork.Repository<PaymentGateway>()
            .GetAllWithIncludesAsync<PaymentGateway>(
                g => g.Id == request.Id,
                g => g,
                g => g.Transactions);

        var gateway = gateways.FirstOrDefault();
        if (gateway == null)
            return Result<PaymentGatewayDto>.Fail("Payment gateway not found.");

        return Result<PaymentGatewayDto>.Ok(PaymentGatewayMapper.ToDto(gateway));
    }
}
