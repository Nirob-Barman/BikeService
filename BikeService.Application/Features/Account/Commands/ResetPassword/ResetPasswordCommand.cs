using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Account.Commands.ResetPassword;

public record ResetPasswordCommand(string? Email, string? Token, string? NewPassword) : IRequest<Result<bool>>;
