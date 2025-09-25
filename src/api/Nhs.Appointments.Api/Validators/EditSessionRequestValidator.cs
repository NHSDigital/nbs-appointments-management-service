using FluentValidation;
using Nhs.Appointments.Api.Models;
using System;

namespace Nhs.Appointments.Api.Validators;
public class EditSessionRequestValidator : AbstractValidator<EditSessionRequest>
{
    public EditSessionRequestValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.Site)
            .NotEmpty()
            .WithMessage("Provide a valid site.");
        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To)
            .WithMessage("From date must be before to date.")
            .GreaterThan(DateOnly.Parse(timeProvider.GetUtcNow().ToString("yyyy-MM-dd")))
            .WithMessage("Date must be in the future.");
        RuleFor(x => x.To)
            .GreaterThanOrEqualTo(x => x.From)
            .WithMessage("To date must be after From date.")
            .GreaterThan(DateOnly.Parse(timeProvider.GetUtcNow().ToString("yyyy-MM-dd")))
            .WithMessage("Date must be in the future.");
        RuleFor(x => x.SessionMatcher)
            .SetValidator(new SessionOrWildcardValidator());
        RuleFor(x => x.SessionReplacement)
            .SetValidator(new SessionValidator())
            .When(x => x.SessionReplacement is not null);
    }
}
