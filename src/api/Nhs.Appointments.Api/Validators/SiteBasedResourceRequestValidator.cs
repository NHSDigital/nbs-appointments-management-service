using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class SiteBasedResourceRequestValidator : AbstractValidator<SiteBasedResourceRequest>
{ 
    public SiteBasedResourceRequestValidator()
    {
        When(x => x.ForUser, () =>
        {
            RuleFor(x => x.Site).Empty().WithMessage("Site id cannot be specified for user based requests");
        }).Otherwise(() =>
        {
            RuleFor(x => x.Site).NotEmpty().WithMessage("Provide a valid site");
        });
    }
}