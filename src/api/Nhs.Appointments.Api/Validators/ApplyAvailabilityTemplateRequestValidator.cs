using System;
using FluentValidation;
using Nhs.Appointments.Api.Availability;

namespace Nhs.Appointments.Api.Validators;

public class ApplyAvailabilityTemplateRequestValidator : AbstractValidator<ApplyAvailabilityTemplateRequest>
{
    public ApplyAvailabilityTemplateRequestValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.Site)            
            .NotEmpty()
            .WithMessage("Provide a valid site");
        
        RuleFor(x => x.From).Cascade(CascadeMode.Stop)
            .LessThanOrEqualTo(x => x.Until)
            .WithMessage("'until' date must be after 'from' date")
            .GreaterThanOrEqualTo(DateOnly.Parse(timeProvider.GetUtcNow().AddDays(1).ToString("yyyy-MM-dd")))
            .WithMessage("'from' date must be at least 1 day in the future");

        RuleFor(x => x.Until)
            .LessThanOrEqualTo(DateOnly.Parse(timeProvider.GetUtcNow().AddYears(1).ToString("yyyy-MM-dd")))
            .WithMessage("'until' date cannot be later than 1 year from now");                                        

        RuleFor(x => x.Template)
            .NotEmpty()
            .WithMessage("Please provide a valid template")
            .SetValidator(new TemplateValidator());
    }
}
