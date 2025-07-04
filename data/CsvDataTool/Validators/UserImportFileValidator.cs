using FluentValidation;

namespace CsvDataTool.Validators;

public class UserImportFileValidator : AbstractValidator<List<UserImportRow>>
{
    public UserImportFileValidator()
    {
        RuleForEach(x => x).SetValidator(new UserImportRowValidator());

        RuleFor(x => x)
            .NotEmpty()
            .WithMessage("Must upload at least one row")
            .Must(lines =>
            {
                return lines
                    .GroupBy(line => new { line.UserId, line.SiteId })
                    .All(g => g.Count() == 1);
            })
            .WithMessage("File contains duplicate rows");
    }
}
