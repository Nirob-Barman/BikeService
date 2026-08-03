using BikeService.Application.DTOs.Payment;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Payments.Queries.GetCheckoutInfo;

public record GetCheckoutInfoQuery(int InvoiceId, string? PromoCode) : IRequest<Result<CheckoutInfoDto>>;
