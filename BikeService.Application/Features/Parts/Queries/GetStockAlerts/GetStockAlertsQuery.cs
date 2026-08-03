using BikeService.Application.DTOs.Part;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Parts.Queries.GetStockAlerts;

public record GetStockAlertsQuery(bool UnresolvedOnly = true) : IRequest<Result<List<PartStockAlertDto>>>;
