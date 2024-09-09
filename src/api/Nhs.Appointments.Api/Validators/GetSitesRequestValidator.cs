﻿using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class GetSitesRequestValidator : AbstractValidator<GetSitesRequest>
{
    public GetSitesRequestValidator()
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
            .NotEmpty()
            .LessThanOrEqualTo(100000)
            .GreaterThanOrEqualTo(0)
            .Configure(rule => rule.MessageBuilder = _ => "Provide a search radius in meters (between 0 - 100,000m).");
        RuleFor(x => x.maxiumRecords)
            .NotEmpty()
            .LessThanOrEqualTo(50)
            .GreaterThanOrEqualTo(0)
            .Configure(rule => rule.MessageBuilder = _ => "Provide a number of maximum records (between 0 - 50 records).");
    }
}