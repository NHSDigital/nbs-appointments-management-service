using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class SetSiteDetailsValidator : AbstractValidator<SetSiteDetailsRequest>
{
    private const string TelephoneRegex = @"^[+0-9 ]*$";

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
            .Matches(TelephoneRegex).WithMessage("Phone number must contain numbers and spaces only");
        RuleFor(x => x.Latitude)
            .Must(x => decimal.TryParse(x, out _)).WithMessage("Latitude must be a decimal number")
            .NotEmpty()
            .WithMessage("Provide a valid latitude");
        RuleFor(x => x.Longitude)
            .Must(x => decimal.TryParse(x, out _)).WithMessage("Longitude must be a decimal number")
            .NotEmpty()
            .WithMessage("Provide a valid longitude");
    }
}
