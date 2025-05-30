using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class SetSiteDetailsRequestValidator : AbstractValidator<SetSiteDetailsRequest>
{
    private const string TelephoneRegex = @"^[+0-9 ]*$";

    public SetSiteDetailsRequestValidator()
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
        RuleFor(x => x.LatitudeDecimal)
            .NotNull()
            .WithMessage("Latitude must be a decimal number")
            .GreaterThanOrEqualTo((decimal)49.8)
            .WithMessage("Latitude must be greater than or equal to 49.8")
            .LessThanOrEqualTo((decimal)60.9)
            .WithMessage("Latitude must be less than or equal to 60.9");
        RuleFor(x => x.LongitudeDecimal)
            .NotNull()
            .WithMessage("Longitude must be a decimal number")
            .GreaterThanOrEqualTo((decimal)-8.1)
            .WithMessage("Longitude must be greater than or equal to -8.1")
            .LessThanOrEqualTo((decimal)1.8)
            .WithMessage("Longitude must be less than or equal to 1.8");
    }
}
