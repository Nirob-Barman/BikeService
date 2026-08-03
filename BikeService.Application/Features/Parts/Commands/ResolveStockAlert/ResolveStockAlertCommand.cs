using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Parts.Commands.ResolveStockAlert;

public record ResolveStockAlertCommand(int Id) : IRequest<Result<bool>>;
