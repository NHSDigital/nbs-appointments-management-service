using System;
using System.Globalization;
using FluentValidation;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Validators;

public class SessionValidator : AbstractValidator<Session>
{
    public SessionValidator()
    {
        RuleFor(x => x.From).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(x => TimeOnly.TryParseExact(x.ToString(), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var _))
            .WithMessage("Provide a 'from' time in the format 'HH-mm'")
            .LessThanOrEqualTo(x => x.Until)
            .WithMessage("'until' time must be after 'from' time");
        RuleFor(x => x.Until).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(x => TimeOnly.TryParseExact(x.ToString(), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var _)).WithMessage("Provide a 'until' time in the format 'HH-mm'").LessThanOrEqualTo(x => x.Until)
            .WithMessage("'until' time must be after 'from' time")
            .GreaterThanOrEqualTo(x => x.From.AddMinutes(x.SlotLength))
            .WithMessage("At least one slot must be available");
        RuleFor(x => x.Capacity)
            .NotEmpty()
            .WithMessage("'capacity' cannot be zero or empty");
        RuleFor(x => x.SlotLength)
            .NotEmpty()
            .WithMessage("'slotLength' cannot be zero or empty");
        RuleFor(x => x.Services)
            .NotEmpty()
            .WithMessage("'services' cannot be empty");
        RuleForEach(x => x.Services)
            .NotEmpty()
            .WithMessage("Provide at least one service value");
    }
}
