using BikeService.Application.DTOs.PaymentGateway;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.PaymentGateways.Queries.GetPaymentGateways;

public class GetPaymentGatewaysQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetPaymentGatewaysQuery, Result<List<PaymentGatewayDto>>>
{
    public async Task<Result<List<PaymentGatewayDto>>> Handle(GetPaymentGatewaysQuery request, CancellationToken cancellationToken)
    {
        var gateways = await unitOfWork.Repository<PaymentGateway>()
            .GetAllWithIncludesAsync<PaymentGateway>(
                g => g,
                g => g.Transactions);

        var dtos = gateways.Select(PaymentGatewayMapper.ToDto).ToList();
        return Result<List<PaymentGatewayDto>>.Ok(dtos);
    }
}
