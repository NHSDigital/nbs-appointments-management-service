using FluentValidation;
using Nhs.Appointments.Api.Models;
using System.Text.RegularExpressions;

namespace Nhs.Appointments.Api.Validators;

public class FindSitesByPostCodeRequestValidator : AbstractValidator<FindSitesByPostCodeRequest>
{
    public FindSitesByPostCodeRequestValidator() 
    {
        RuleFor(x => x.postCode).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Provide a postcode")
                .Must(BeAValidPostcode).WithMessage("Provide a valid postcode");
    }

    private bool BeAValidPostcode(string postcode)
    {
        if (string.IsNullOrWhiteSpace(postcode)) return false;
        var trimmedPostcode = postcode.Replace(" ", string.Empty);
        return Regex.IsMatch(trimmedPostcode, @"^(([gG][iI][rR] {0,}0[aA]{2})|((([a-pr-uwyzA-PR-UWYZ][a-hk-yA-HK-Y]?[0-9][0-9]?)|(([a-pr-uwyzA-PR-UWYZ][0-9][a-hjkstuwA-HJKSTUW])|([a-pr-uwyzA-PR-UWYZ][a-hk-yA-HK-Y][0-9][abehmnprv-yABEHMNPRV-Y]))) {0,}[0-9][abd-hjlnp-uw-zABD-HJLNP-UW-Z]{2}))$");
    }
}
