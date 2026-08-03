using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Parts.Commands.DeletePart;

public record DeletePartCommand(int Id) : IRequest<Result<bool>>;
