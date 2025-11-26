using FluentValidation;
using Nhs.Appointments.Api.Models;
using System;

namespace Nhs.Appointments.Api.Validators;
public class AvailabilityQueryByHoursRequestValidator : AbstractValidator<AvailabilityQueryByHoursRequest>
{
    public AvailabilityQueryByHoursRequestValidator(TimeProvider timeProvider)
    {
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
            .WithMessage("Provide a date in 'yyyy-MM-dd'")
            .LessThanOrEqualTo(x => x.Until)
            .WithMessage("'from' date must be before 'until' date")
            .GreaterThan(DateOnly.Parse(timeProvider.GetUtcNow().ToString("yyyy-MM-dd")))
            .WithMessage("From date must be in the future.");
        RuleFor(x => x.Until)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Provide an until date in 'yyyy-MM-dd'")
            .GreaterThanOrEqualTo(x => x.From)
            .WithMessage("'until' date must be after 'from' date")
            .GreaterThan(DateOnly.Parse(timeProvider.GetUtcNow().ToString("yyyy-MM-dd")))
            .WithMessage("Until date must be in the future.");
    }
}
