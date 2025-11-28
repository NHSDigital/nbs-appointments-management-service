using FluentValidation;
using Nhs.Appointments.Api.Models;
using System;

namespace Nhs.Appointments.Api.Validators;
public class AvailabilityQueryBySlotsRequestValidator : AbstractValidator<AvailabilityQueryBySlotsRequest>
{
    public AvailabilityQueryBySlotsRequestValidator(TimeProvider timeProvider)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        RuleFor(x => x.Site)
            .NotEmpty()
            .WithMessage("Provide a site.");
        RuleFor(x => x.Attendees)
            .NotEmpty()
            .WithMessage("Provide a list of attendees.")
            .Must(x => x.Count <= 5)
            .WithMessage("Only a maximum of 5 attendees are allowed.");
        RuleForEach(x => x.Attendees)
            .SetValidator(new AttendeeValidator());
        RuleFor(x => x.From)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Provide a from date time.")
            .GreaterThan(now)
            .WithMessage("From date time must be in the future.");
        RuleFor(x => x.Until)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Provide an until date time.")
            .GreaterThan(now)
            .WithMessage("Until date time must be in the future.");
        RuleFor(x => x)
            .Must(x => x.From.Date == x.Until.Date)
            .WithMessage("From date and until date must be on the same day.")
            .Must(x => x.From < x.Until)
            .WithMessage("'From' must be earlier than 'Until'");
    }
}
