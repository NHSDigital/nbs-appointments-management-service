using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class SetSiteDetailsValidator : AbstractValidator<SetSiteDetailsRequest>
{
    private const string NumbersOnlyRegex = @"^\d+$";
    private const string DecimalRegex = @"^-?\d+(\.\d+)?$";

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
            .Matches(NumbersOnlyRegex).WithMessage("Phone number must contain numbers only")
            .NotEmpty().WithMessage("Provide a valid phone number");
        RuleFor(x => x.Latitude)
            .Matches(DecimalRegex).WithMessage("Latitude must be a decimal number")
            .NotEmpty()
            .WithMessage("Provide a valid latitude");
        RuleFor(x => x.Longitude)
            .Matches(DecimalRegex).WithMessage("Longitude must be a decimal number")
            .NotEmpty()
            .WithMessage("Provide a valid longitude");
    }
}
