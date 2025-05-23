using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;
public class SetSiteInformationForCitizensValidator : AbstractValidator<SetSiteInformationForCitizensRequest>
{
    public SetSiteInformationForCitizensValidator()
    {
        RuleFor(x => x.Site)
            .NotEmpty()
            .WithMessage("Provide a valid site");
    }
}
