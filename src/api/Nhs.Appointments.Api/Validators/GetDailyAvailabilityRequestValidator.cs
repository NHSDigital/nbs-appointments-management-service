using FluentValidation;
using Nhs.Appointments.Api.Models;
using System;

namespace Nhs.Appointments.Api.Validators
{
    public class GetDailyAvailabilityRequestValidator : AbstractValidator<GetDailyAvailabilityRequest>
    {
        public GetDailyAvailabilityRequestValidator()
        {
            RuleFor(x => x.Site)
            .NotEmpty()
            .WithMessage("Provide a valid site");

            RuleFor(x => x.From)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Provide a date in 'yyyy-MM-dd'")
                .Must(x => DateOnly.TryParseExact(x, "yyyy-MM-dd", out var _))
                .WithMessage("Provide a date in the format 'yyyy-MM-dd'")
                .LessThanOrEqualTo(x => x.Until)
                .WithMessage("'from' date must be before 'to' date");

            RuleFor(x => x.Until)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Provide a date in 'yyyy-MM-dd'")
                .Must(x => DateOnly.TryParseExact(x, "yyyy-MM-dd", out var _))
                .WithMessage("Provide a date in the format 'yyyy-MM-dd'");
        }
    }
}
