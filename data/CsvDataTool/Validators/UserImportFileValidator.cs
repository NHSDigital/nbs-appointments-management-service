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
            .Must(x => x.Count == x.Distinct().ToList().Count)
            .WithMessage("File contains duplicate rows");
    }
}
