using FluentValidation;
using Nhs.Appointments.Core.Sites;
using System;
using System.Linq;

namespace Nhs.Appointments.Api.Validators;
public class SiteFilterValidator : AbstractValidator<SiteFilter>
{
    public SiteFilterValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.Longitude)
            .LessThanOrEqualTo(180)
            .GreaterThanOrEqualTo(-180)
            .WithMessage("Provide a valid longitude value (between -180 <-> 180 degrees).");
        RuleFor(x => x.Latitude)
            .LessThanOrEqualTo(90)
            .GreaterThanOrEqualTo(-90)
            .WithMessage("Provide a valid latitude value (between -90 <-> 90 degrees).");
        RuleFor(x => x.SearchRadius)
            .LessThanOrEqualTo(100000)
            .GreaterThanOrEqualTo(1000)
            .WithMessage("Provide a search radius in meters (between 1000 - 100,000m).");
        RuleFor(x => x.Availability)
            .SetValidator(new AvailabilityFilterValidator(timeProvider))
            .When(x => x.Availability is not null);
        RuleFor(x => x.Priority)
            .GreaterThan(0)
            .WithMessage("Priority must be greater than zero.")
            .When(x => x.Priority is not null);
    }
}
