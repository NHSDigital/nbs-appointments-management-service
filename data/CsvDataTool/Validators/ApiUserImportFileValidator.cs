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
            .Must(lines =>
            {
                return lines.All(line => lines.GroupBy(l => l == line).Count() <= 1);
            })
            .WithMessage("File contains duplicate rows");
    }
}
