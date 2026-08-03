using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Account.Commands.ChangePassword;

public record ChangePasswordCommand(string? CurrentPassword, string? NewPassword) : IRequest<Result<bool>>;
