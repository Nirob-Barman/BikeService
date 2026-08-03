using BikeService.Application.DTOs.Part;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Parts.Queries.GetStockAlerts;

public class GetStockAlertsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetStockAlertsQuery, Result<List<PartStockAlertDto>>>
{
    public async Task<Result<List<PartStockAlertDto>>> Handle(GetStockAlertsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<PartStockAlert> alerts;

        if (request.UnresolvedOnly)
            alerts = await unitOfWork.Repository<PartStockAlert>().Where(a => !a.IsResolved);
        else
            alerts = await unitOfWork.Repository<PartStockAlert>().GetAllAsync();

        var partIds = alerts.Select(a => a.PartId).Distinct().ToList();
        var parts = await unitOfWork.Repository<Part>().Where(p => partIds.Contains(p.Id));
        var partDict = parts.ToDictionary(p => p.Id);

        var dtos = alerts
            .Where(a => partDict.ContainsKey(a.PartId))
            .Select(a => PartMapper.ToStockAlertDto(a, partDict[a.PartId]))
            .ToList();

        return Result<List<PartStockAlertDto>>.Ok(dtos);
    }
}
