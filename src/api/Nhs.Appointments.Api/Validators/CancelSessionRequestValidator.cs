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

        RuleFor(x => x.Date)
            .GreaterThanOrEqualTo(DateOnly.Parse(timeProvider.GetUtcNow().ToString("yyyy-MM-dd")))
            .WithMessage("From date must be in the future.");

        RuleFor(x => x.From)
            .NotEmpty()
            .WithMessage("Provide a from time");

        RuleFor(x => x.Until)
            .NotEmpty()
            .WithMessage("Provide an until time");

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
