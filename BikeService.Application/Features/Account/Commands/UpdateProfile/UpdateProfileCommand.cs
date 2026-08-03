using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Account.Commands.UpdateProfile;

public record UpdateProfileCommand(string? FirstName, string? LastName, string? Address) : IRequest<Result<bool>>;
