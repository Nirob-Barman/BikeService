using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Parts.Commands.CreatePart;

public record CreatePartCommand(
    string Name,
    string SKU,
    decimal UnitPrice,
    int StockQuantity,
    int LowStockThreshold) : IRequest<Result<int>>;
