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
        RuleFor(x => x.Date)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Provide a date in 'yyyy-MM-dd'")
            .GreaterThan(DateOnly.Parse(timeProvider.GetUtcNow().ToString("yyyy-MM-dd")))
            .WithMessage("Date must be in the future.");
    }
}
