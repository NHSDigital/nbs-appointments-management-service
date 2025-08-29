using FluentValidation;
using Nhs.Appointments.Api.Models;
using System;

namespace Nhs.Appointments.Api.Validators;
public class CancelDayRequestValidator : AbstractValidator<CancelDayRequest>
{
    public CancelDayRequestValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.Site)
            .NotEmpty()
            .WithMessage("Provide a valid site.");

        RuleFor(x => x.Date)
            .GreaterThan(DateOnly.Parse(timeProvider.GetUtcNow().ToString("yyyy-MM-dd")))
            .WithMessage("Date must be in the future.");
    }
}
