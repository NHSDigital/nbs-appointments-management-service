using FluentValidation;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Validators;

public class SiteConfigurationValidator :  AbstractValidator<SiteConfiguration>
{
    public SiteConfigurationValidator() 
    {
        RuleFor(x => x.Site)
            .NotEmpty().WithMessage("Provide a site");
    } 
}