using BikeService.Application.DTOs.Identity;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Account.Queries.GetProfile;

public record GetProfileQuery : IRequest<Result<EditProfileDto>>;
