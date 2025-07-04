using FluentValidation;

namespace CsvDataTool.Validators;

public class ApiUserImportFileValidator : AbstractValidator<List<ApiUserImportRow>>
{
    public ApiUserImportFileValidator()
    {
        RuleForEach(x => x).SetValidator(new ApiUserImportRowValidator());

        RuleFor(x => x)
            .NotEmpty()
            .WithMessage("Must upload at least one row")
            .Must(x => x.Count == x.Distinct().ToList().Count)
            .WithMessage("File contains duplicate rows");
    }
}
