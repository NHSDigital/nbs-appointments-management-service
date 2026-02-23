using FluentValidation;
using Nhs.Appointments.Api.Models;
using System;

namespace Nhs.Appointments.Api.Validators;

public class ProposeCancelDateRangeRequestValidator : AbstractValidator<ProposeCancelDateRangeRequest>
{
    public ProposeCancelDateRangeRequestValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.Site)
            .NotEmpty()
            .WithMessage("Site is required.");
        RuleFor(x => x.From)
            .NotEmpty()
            .WithMessage("From date is required.")
            .LessThan(x => x.To)
            .WithMessage("From date must be before To date.")
            .GreaterThan(DateOnly.Parse(timeProvider.GetUtcNow().ToString("yyyy-MM-dd")))
            .WithMessage("Date must be in the future.")
            .LessThan(DateOnly.Parse(timeProvider.GetUtcNow().ToString("yyyy-MM-dd")).AddDays(90))
            .WithMessage("Date cannot be more than 90 days in the future.");
        RuleFor(x => x.To)
            .NotEmpty()
            .WithMessage("To date is required.")
            .GreaterThan(x => x.From)
            .WithMessage("To date must be after From date.")
            .GreaterThan(DateOnly.Parse(timeProvider.GetUtcNow().ToString("yyyy-MM-dd")))
            .WithMessage("Date must be in the future.")
            .LessThan(DateOnly.Parse(timeProvider.GetUtcNow().ToString("yyyy-MM-dd")).AddDays(90))
            .WithMessage("Date cannot be more than 90 days in the future.")
            .Must((req, until) => Within90Days(until, req.From))
            .WithMessage("To date cannot be more than 90 days after from date.");
    }

    // TODO: Move this value to config in APPT-1987
    private static bool Within90Days(DateOnly until, DateOnly from)
    {
        return until <= from.AddDays(90);
    }
}
