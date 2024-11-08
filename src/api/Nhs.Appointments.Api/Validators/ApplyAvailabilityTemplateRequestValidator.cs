using System;
using System.Globalization;
using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class ApplyAvailabilityTemplateRequestValidator : AbstractValidator<ApplyAvailabilityTemplateRequest>
{
    public ApplyAvailabilityTemplateRequestValidator()
    {
        RuleFor(x => x.Site)            
            .NotEmpty()
            .WithMessage("Provide a valid site");
        RuleFor(x => x.From)
                                .LessThanOrEqualTo(x => x.Until)
                                .WithMessage("'until' date must be after 'from' date");
        RuleFor(x => x.Template)
            .NotEmpty()
            .WithMessage("Please provide a valid template")
            .SetValidator(new TemplateValidator());
    }
}
