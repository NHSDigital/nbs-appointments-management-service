﻿using FluentValidation;
using Nhs.Appointments.Api.Availability;
using System;
using System.Globalization;

namespace Nhs.Appointments.Api.Validators
{
    public class SetAvailabilityRequestValidator : AbstractValidator<SetAvailabilityRequest>
    {
        public SetAvailabilityRequestValidator(TimeProvider timeProvider)
        {
            RuleFor(x => x.Date).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(x => DateOnly.TryParseExact(x.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var _))
                .WithMessage("Provide a date in the format 'yyyy-MM-dd'").DependentRules(() =>
                {
                    RuleFor(x => x.AvailabilityDate)
                        .GreaterThanOrEqualTo(DateOnly.Parse(timeProvider.GetUtcNow().AddDays(1).ToString("yyyy-MM-dd")))
                        .WithMessage("Date must be at least 1 day in the future")
                        .LessThanOrEqualTo(DateOnly.Parse(timeProvider.GetUtcNow().AddYears(1).ToString("yyyy-MM-dd")))
                        .WithMessage("Date cannot be later than 1 year from now");
                });

            RuleFor(x => x.Site)
                .NotEmpty()
                .WithMessage("Provide a valid site.");

            RuleFor(x => x.Sessions)
                .NotEmpty()
                .WithMessage("Provide one or more valid sessions");

            RuleForEach(x => x.Sessions)
                .SetValidator(new SessionValidator());
        }
    }
}
