using FluentValidation;
using Nhs.Appointments.Core;
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
        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(x =>
                (
                    //none of the filters are provided
                    (x.Services is null || x.Services.Length is 0) &&
                    x.From is null && x.Until is null)
                || //OR
                   //all of the filters are provided together
                (x.Services?.Length > 0 && x.From is not null && x.Until is not null)
            )
            .WithMessage(
                "All of the 'supports service filters' (services, from, until) must be provided, or none of them.")
            .DependentRules(() =>
            {
                When(x => x.Services?.Length > 0 && x.From is not null && x.Until is not null,
                    () =>
                    {
                        RuleFor(x => x.Services)
                            .Must(services => services.Length == 1 && (services.Single() == "RSV:Adult" || services.Single() == "FLU:2_3" || services.Single() == "COVID:5_11" || services.Single() == "COVID:12_17"))
                            .WithMessage("'Services' currently only supports: 'RSV:Adult or 'FLU:2_3' or  'COVID:5_11' or 'COVID:12_17'");
                        RuleFor(x => x.From)
                            .LessThanOrEqualTo(x => x.Until)
                            .WithMessage("From date must be before to date.")
                            .GreaterThan(DateOnly.Parse(timeProvider.GetUtcNow().ToString("yyyy-MM-dd")))
                            .WithMessage("Date must be in the future.");
                        RuleFor(x => x.Until)
                            .GreaterThanOrEqualTo(x => x.From)
                            .WithMessage("To date must be after From date.")
                            .GreaterThan(DateOnly.Parse(timeProvider.GetUtcNow().ToString("yyyy-MM-dd")))
                            .WithMessage("Date must be in the future.");
                    });
            });
        RuleFor(x => x.Priority)
            .GreaterThan(0)
            .WithMessage("Priority must be greater than zero.")
            .When(x => x.Priority is not null);
    }
}
