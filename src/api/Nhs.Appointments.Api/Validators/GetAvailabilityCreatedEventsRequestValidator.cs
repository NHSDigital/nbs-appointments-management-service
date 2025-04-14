using System;
using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class GetAvailabilityCreatedEventsRequestValidator : AbstractValidator<GetAvailabilityCreatedEventsRequest>
{
    public GetAvailabilityCreatedEventsRequestValidator()
    {
        RuleFor(x => x.Site)
            .NotEmpty()
            .WithMessage("Provide a valid site");

        RuleFor(x => x.From)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Provide a date in dayStringFormat")
            .Must(x => DateOnly.TryParseExact(x, "yyyy-MM-dd", out var _))
            .WithMessage("Provide a date in the format dayStringFormat");
    }
}
