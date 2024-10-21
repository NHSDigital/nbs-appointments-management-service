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
        RuleFor(x => x.From).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(x => DateOnly.TryParseExact(x.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var _))
            .WithMessage("Provide a from date in the format 'yyyy-MM-dd'")
            .DependentRules(
                () =>
                {
                    RuleFor(x => x.Until).Cascade(CascadeMode.Stop)
                        .NotEmpty()
                        .Must(x => DateOnly.TryParseExact(x.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var _))
                        .WithMessage("Provide a from date in the format 'yyyy-MM-dd'")
                        .DependentRules(
                        () =>
                        {
                            RuleFor(x => x.FromDate).Cascade(CascadeMode.Stop)
                                .LessThanOrEqualTo(x => x.UntilDate)
                                .WithMessage("'until' date must be after 'from' date");
                        });
                });
        RuleFor(x => x.Template)
            .NotEmpty()
            .WithMessage("Please provide a valid template")
            .SetValidator(new TemplateValidator());
    }
}
