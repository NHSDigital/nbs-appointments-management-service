using System;
using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class GetSitesByAreaRequestValidator : AbstractValidator<GetSitesByAreaRequest>
{
    public GetSitesByAreaRequestValidator()
    {
        RuleFor(x => x.longitude)
            .LessThanOrEqualTo(180)
            .GreaterThanOrEqualTo(-180)
            .Configure(rule =>
                rule.MessageBuilder = _ => "Provide a valid longitude value (between -180 <-> 180 degrees).");
        RuleFor(x => x.latitude)
            .LessThanOrEqualTo(90)
            .GreaterThanOrEqualTo(-90)
            .Configure(rule =>
                rule.MessageBuilder = _ => "Provide a valid latitude value (between -90 <-> 90 degrees).");
        RuleFor(x => x.searchRadius)
            .LessThanOrEqualTo(100000)
            .GreaterThanOrEqualTo(1000)
            .Configure(rule =>
                rule.MessageBuilder = _ => "Provide a search radius in meters (between 1000 - 100,000m).");
        RuleFor(x => x.maximumRecords)
            .LessThanOrEqualTo(50)
            .GreaterThan(0)
            .Configure(rule =>
                rule.MessageBuilder = _ => "Provide a number of maximum records (between 1 - 50 records).");
        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(x =>
                (
                    //none of the filters are provided
                    (x.services == null || x.services.Length == 0) &&
                    string.IsNullOrEmpty(x.from) & string.IsNullOrEmpty(x.until))
                || //OR
                //all of the filters are provided together
                (x.services?.Length > 0 && !string.IsNullOrEmpty(x.from) && !string.IsNullOrEmpty(x.until))
            )
            .WithMessage(
                "All of the 'supports service filters' (services, from, until) must be provided, or none of them.")
            .DependentRules(() =>
            {
                When(x => x.services?.Length > 0 && !string.IsNullOrEmpty(x.from) && !string.IsNullOrEmpty(x.until),
                    () =>
                    {
                        RuleFor(x => x.services)
                            .Must(services => services.Length == 1)
                            .WithMessage("'Services' currently only supports one service");
                        RuleFor(x => x)
                            .Cascade(CascadeMode.Stop)
                            .Must(x => DateOnly.TryParseExact(x.from, "yyyy-MM-dd", out _))
                            .WithMessage("'From' must be a date in the format 'yyyy-MM-dd'")
                            .Must(x => DateOnly.TryParseExact(x.until, "yyyy-MM-dd", out _))
                            .WithMessage("'Until' must be a date in the format 'yyyy-MM-dd'")
                            .Must(x =>
                            {
                                var fromDate = DateOnly.ParseExact(x.from, "yyyy-MM-dd");
                                var untilDate = DateOnly.ParseExact(x.until, "yyyy-MM-dd");
                                return untilDate >= fromDate;
                            })
                            .WithMessage("'Until' date must be greater than or equal to 'From' date");
                    });
            });
    }
}
