using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class GetSitesByAreaRequestValidator : AbstractValidator<GetSitesRequest>
{
    public GetSitesByAreaRequestValidator()
    {
        RuleFor(x => x.longitude)
            .NotEmpty()
            .LessThanOrEqualTo(180)
            .GreaterThanOrEqualTo(-180)
            .Configure(rule => rule.MessageBuilder = _ => "Provide a valid longitude value (between -180 <-> 180 degrees).");
        RuleFor(x => x.latitude)
            .NotEmpty()
            .LessThanOrEqualTo(90)
            .GreaterThanOrEqualTo(-90)
            .Configure(rule => rule.MessageBuilder = _ => "Provide a valid latitude value (between -90 <-> 90 degrees).");
        RuleFor(x => x.searchRadius)
            .NotNull()
            .LessThanOrEqualTo(100000)
            .GreaterThanOrEqualTo(1000)
            .Configure(rule => rule.MessageBuilder = _ => "Provide a search radius in meters (between 1000 - 100,000m).");
        RuleFor(x => x.maximumRecords)
            .NotNull()
            .LessThanOrEqualTo(50)
            .GreaterThan(0)
            .Configure(rule => rule.MessageBuilder = _ => "Provide a number of maximum records (between 1 - 50 records).");
    }
}