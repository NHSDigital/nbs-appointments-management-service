using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class SetSiteAccessibilityValuesValidator : AbstractValidator<SetSiteAccessibilitiesRequest>
{
    public SetSiteAccessibilityValuesValidator()
    {
        RuleFor(x => x.Site)
            .NotEmpty()
            .WithMessage("Provide a valid site");
        RuleFor(x => x.Accessibilities)
            .NotEmpty()
            .WithMessage("Accessibility values must be provided");
        RuleForEach(x => x.Accessibilities)
            .SetValidator(new AccessibilityValidator());
    }
}
