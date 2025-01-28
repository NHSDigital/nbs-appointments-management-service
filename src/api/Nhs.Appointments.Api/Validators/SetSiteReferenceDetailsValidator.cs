using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class SetSiteReferenceDetailsValidator : AbstractValidator<SetSiteReferenceDetailsRequest>
{
    public SetSiteReferenceDetailsValidator()
    {
        RuleFor(x => x.Site)
            .NotEmpty()
            .WithMessage("Provide a valid site");
        RuleFor(x => x.OdsCode)
            .NotEmpty()
            .WithMessage("Provide a valid ODS code");
        RuleFor(x => x.Icb)
            .NotEmpty()
            .WithMessage("Provide a valid ICB");
        RuleFor(x => x.Region)
            .NotEmpty()
            .WithMessage("Provide a valid region");
    }
}
