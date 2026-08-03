using FluentValidation;

namespace BikeService.Application.Features.Parts.Commands.ImportParts;

public class ImportPartsCommandValidator : AbstractValidator<ImportPartsCommand>
{
    public ImportPartsCommandValidator()
    {
        RuleFor(x => x.CsvStream).NotNull();
        RuleFor(x => x.FileName).NotEmpty();
    }
}
