using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Parts.Commands.UpdatePart;

public record UpdatePartCommand(
    int Id,
    string Name,
    string SKU,
    decimal UnitPrice,
    int StockQuantity,
    int LowStockThreshold) : IRequest<Result<bool>>;
