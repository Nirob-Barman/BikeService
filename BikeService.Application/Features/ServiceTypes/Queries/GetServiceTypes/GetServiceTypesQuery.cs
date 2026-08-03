using BikeService.Application.DTOs.ServiceType;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.ServiceTypes.Queries.GetServiceTypes;

public record GetServiceTypesQuery : IRequest<Result<List<ServiceTypeDto>>>;
