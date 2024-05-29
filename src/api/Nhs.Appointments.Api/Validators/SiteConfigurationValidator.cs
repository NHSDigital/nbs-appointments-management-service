using FluentValidation;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Validators;

public class SiteConfigurationValidator :  AbstractValidator<SiteConfiguration>
{
    public SiteConfigurationValidator() 
    {
        RuleFor(x => x.SiteId)
            .NotEmpty().WithMessage("Provide a site id");
    } 
}