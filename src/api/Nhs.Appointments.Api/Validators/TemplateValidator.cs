using System;
using System.Linq;
using FluentValidation;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Validators;

public class TemplateValidator : AbstractValidator<Template>
{
    public TemplateValidator()
    {
        RuleFor(x => x.Days)
            .NotEmpty()
            .WithMessage("Provide at least one day")
            .Must(SpecifyEachDayOnlyOnce)
            .WithMessage("A day can only appear once");
        RuleFor(x => x.Sessions)
            .NotEmpty()
            .WithMessage("Provide a valid session");
        RuleForEach(x => x.Sessions)
            .SetValidator(new SessionValidator());
    }
    
    private static bool SpecifyEachDayOnlyOnce(DayOfWeek[] days)
    {
        var allDays = days.ToList();
        return allDays.Count == allDays.Distinct().Count();
    }
}
