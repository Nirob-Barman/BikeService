using BikeService.Application.DTOs.BulkImport;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Parts.Commands.ImportParts;

public record ImportPartsCommand(Stream CsvStream, string FileName) : IRequest<Result<BulkImportResultDto>>;
