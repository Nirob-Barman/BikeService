using BikeService.Application.DTOs.ServiceType;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.ServiceTypes.Queries.GetActiveServiceTypes;

public record GetActiveServiceTypesQuery : IRequest<Result<List<ServiceTypeDto>>>;
