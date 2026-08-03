using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.PromoCodes.Commands.DeletePromoCode;

public record DeletePromoCodeCommand(int Id) : IRequest<Result<bool>>;
