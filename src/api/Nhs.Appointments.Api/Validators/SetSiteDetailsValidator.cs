using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class SetSiteDetailsValidator : AbstractValidator<SetSiteDetailsRequest>
{
    public SetSiteDetailsValidator()
    {
        RuleFor(x => x.Site)
            .NotEmpty()
            .WithMessage("Provide a valid site");
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Provide a valid name");
        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage("Provide a valid address");
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Provide a valid phone number");
        RuleFor(x => x.Latitude)
            .NotEmpty()
            .WithMessage("Provide a valid latitude");
        RuleFor(x => x.Longitude)
            .NotEmpty()
            .WithMessage("Provide a valid longitude");
    }
}
