using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Account.Commands.ForgotPassword;

public record ForgotPasswordCommand(string? Email, string BaseUrl) : IRequest<Result<bool>>;
