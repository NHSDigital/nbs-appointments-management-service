using FluentValidation;
using Nhs.Appointments.Persistance.Models;

namespace CsvDataTool.Validators;

public class SiteImportFileValidator : AbstractValidator<List<SiteDocument>>
{
    public SiteImportFileValidator()
    {
        RuleForEach(x => x).SetValidator(new SiteImportRowValidator());

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
