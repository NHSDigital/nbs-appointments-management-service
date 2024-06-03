using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class SiteBasedResourceRequestValidator : AbstractValidator<SiteBasedResourceRequest>
{ 
    public SiteBasedResourceRequestValidator()
    {        
        RuleFor(x => x.Site).NotEmpty().WithMessage("Provide a valid site");        
    }
}