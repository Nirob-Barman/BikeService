using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Account.Commands.UploadProfilePhoto;

public record UploadProfilePhotoCommand(Stream PhotoStream, string FileName) : IRequest<Result<bool>>;
