using FluentValidation;
using Nhs.Appointments.Api.Models;
using System;

namespace Nhs.Appointments.Api.Validators;
public class CancelSessionRequestValidator : AbstractValidator<CancelSessionRequest>
{
    public CancelSessionRequestValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.Site)
            .NotEmpty()
            .WithMessage("Provide a valid site.");

        RuleFor(x => x.From)
            .GreaterThanOrEqualTo(DateTime.Parse(timeProvider.GetUtcNow().ToString("yyyy-MM-dd")))
            .WithMessage("From date must be at least one day in the future.")
            .LessThanOrEqualTo(x => x.Until)
            .WithMessage("From date must be before Until date.");

        RuleFor(x => x.Services)
            .NotEmpty()
            .WithMessage("Provide one or more valid services.");

        RuleFor(x => x.SlotLength)
            .GreaterThan(0)
            .WithMessage("Slot length must be greater than zero.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0)
            .WithMessage("Capacity must be greater than zero.");
    }
}
