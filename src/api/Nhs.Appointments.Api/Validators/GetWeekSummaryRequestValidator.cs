using FluentValidation;
using Nhs.Appointments.Api.Models;
using System;

namespace Nhs.Appointments.Api.Validators
{
    public class GetWeekSummaryRequestValidator : AbstractValidator<GetWeekSummaryRequest>
    {
        public GetWeekSummaryRequestValidator()
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
                .Must(x =>
                {
                    var dateOnly = DateOnly.ParseExact(x, "yyyy-MM-dd");
                    return dateOnly.DayOfWeek == DayOfWeek.Monday;
                })
                .WithMessage("Provide a date that is a Monday");
        }
    }
}
