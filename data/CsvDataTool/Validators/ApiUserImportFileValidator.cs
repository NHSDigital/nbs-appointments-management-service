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
                return lines
                    .GroupBy(line => new { line.ClientId, line.ApiSigningKey })
                    .All(g => g.Count() == 1);
            })
            .WithMessage("File contains duplicate rows");
    }
}
