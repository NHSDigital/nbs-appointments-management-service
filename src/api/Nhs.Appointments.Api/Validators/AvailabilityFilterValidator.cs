using FluentValidation;
using Nhs.Appointments.Core.Sites;
using System;
using System.Linq;

namespace Nhs.Appointments.Api.Validators;
public class AvailabilityFilterValidator : AbstractValidator<AvailabilityFilter>
{
    public AvailabilityFilterValidator(TimeProvider timeProvider)
    {         
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
                            .Must(services => services.Length > 0)
                            .WithMessage("At least one service must be provided.");
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
    }
}
