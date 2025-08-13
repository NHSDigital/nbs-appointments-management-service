using System;
using System.Linq;
using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class GetSitesByAreaRequestValidator : AbstractValidator<GetSitesByAreaRequest>
{
    public GetSitesByAreaRequestValidator()
    {
        RuleFor(x => x.Longitude)
            .LessThanOrEqualTo(180)
            .GreaterThanOrEqualTo(-180)
            .Configure(rule =>
                rule.MessageBuilder = _ => "Provide a valid longitude value (between -180 <-> 180 degrees).");
        RuleFor(x => x.Latitude)
            .LessThanOrEqualTo(90)
            .GreaterThanOrEqualTo(-90)
            .Configure(rule =>
                rule.MessageBuilder = _ => "Provide a valid latitude value (between -90 <-> 90 degrees).");
        RuleFor(x => x.SearchRadius)
            .LessThanOrEqualTo(100000)
            .GreaterThanOrEqualTo(1000)
            .Configure(rule =>
                rule.MessageBuilder = _ => "Provide a search radius in meters (between 1000 - 100,000m).");
        RuleFor(x => x.MaximumRecords)
            .LessThanOrEqualTo(50)
            .GreaterThan(0)
            .Configure(rule =>
                rule.MessageBuilder = _ => "Provide a number of maximum records (between 1 - 50 records).");
        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(x =>
                (
                    //none of the filters are provided
                    (x.Services == null || x.Services.Length == 0) &&
                    string.IsNullOrEmpty(x.From) & string.IsNullOrEmpty(x.Until))
                || //OR
                //all of the filters are provided together
                (x.Services?.Length > 0 && !string.IsNullOrEmpty(x.From) && !string.IsNullOrEmpty(x.Until))
            )
            .WithMessage(
                "All of the 'supports service filters' (services, from, until) must be provided, or none of them.")
            .DependentRules(() =>
            {
                When(x => x.Services?.Length > 0 && !string.IsNullOrEmpty(x.From) && !string.IsNullOrEmpty(x.Until),
                    () =>
                    {
                        RuleFor(x => x.Services)
                            .Must(services => services.Length == 1 && services.Single() == "RSV:Adult")
                            .WithMessage("'Services' currently only supports one service: 'RSV:Adult'");
                        RuleFor(x => x)
                            .Cascade(CascadeMode.Stop)
                            .Must(x => DateOnly.TryParseExact(x.From, "yyyy-MM-dd", out _))
                            .WithMessage("'From' must be a date in the format 'yyyy-MM-dd'")
                            .Must(x => DateOnly.TryParseExact(x.Until, "yyyy-MM-dd", out _))
                            .WithMessage("'Until' must be a date in the format 'yyyy-MM-dd'")
                            .Must(x =>
                            {
                                var fromDate = DateOnly.ParseExact(x.From, "yyyy-MM-dd");
                                var untilDate = DateOnly.ParseExact(x.Until, "yyyy-MM-dd");
                                return untilDate >= fromDate;
                            })
                            .WithMessage("'Until' date must be greater than or equal to 'From' date");
                    });
            });
    }
}
