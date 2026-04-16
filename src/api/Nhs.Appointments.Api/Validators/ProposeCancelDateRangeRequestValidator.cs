using FluentValidation;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Api.Models;
using System;

namespace Nhs.Appointments.Api.Validators;

public class ProposeCancelDateRangeRequestValidator : AbstractValidator<ProposeCancelDateRangeRequest>
{
    public ProposeCancelDateRangeRequestValidator(
        TimeProvider timeProvider,
        IOptions<ChangeAvailabilityOptions> options)
    {
        var maxDays = options.Value.CancelADateRangeMaximumDays;

        RuleFor(x => x.Site)
            .NotEmpty()
            .WithMessage("Site is required.");
        RuleFor(x => x.From)
            .NotEmpty()
            .WithMessage("From date is required.")
            .LessThanOrEqualTo(x => x.To)
            .WithMessage("From date must be before To date.")
            .GreaterThan(DateOnly.Parse(timeProvider.GetUtcNow().ToString("yyyy-MM-dd")))
            .WithMessage("From date must be in the future.");
        RuleFor(x => x.To)
            .NotEmpty()
            .WithMessage("To date is required.")
            .GreaterThanOrEqualTo(x => x.From)
            .WithMessage("To date must be after From date.")
            .GreaterThan(DateOnly.Parse(timeProvider.GetUtcNow().ToString("yyyy-MM-dd")))
            .WithMessage("To date must be in the future.")
            .Must((req, until) => WithinMaxDays(until, req.From, maxDays))
            .WithMessage($"To date has to be less than {maxDays} days after the From date.");
    }

    private static bool WithinMaxDays(DateOnly until, DateOnly from, int maxDays)
    {
        return until < from.AddDays(maxDays);
    }
}
