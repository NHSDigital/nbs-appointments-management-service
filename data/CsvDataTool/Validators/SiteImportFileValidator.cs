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
                return lines
                    .GroupBy(line => new
                    {
                        line.Name,
                        line.Address,
                        line.PhoneNumber,
                        line.OdsCode,
                        line.Region,
                        line.Longitude,
                        line.Latitude,
                        line.InformationForCitizens,
                        line.Accessibilities,
                        line.Type
                    })
                    .All(g => g.Count() == 1);
            })
            .WithMessage("File contains duplicate rows");
    }
}
