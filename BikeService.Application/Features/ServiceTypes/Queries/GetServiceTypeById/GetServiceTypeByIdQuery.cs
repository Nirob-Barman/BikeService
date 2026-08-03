using BikeService.Application.DTOs.ServiceType;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.ServiceTypes.Queries.GetServiceTypeById;

public record GetServiceTypeByIdQuery(int Id) : IRequest<Result<ServiceTypeDto>>;
