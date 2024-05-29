using FluentValidation;
using Nhs.Appointments.Core;
using System.Linq;

namespace Nhs.Appointments.Api.Validators
{
    public class WeekTemplateValidator : AbstractValidator<WeekTemplate>
    {
        public WeekTemplateValidator() 
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Provide a name for the template");
            RuleFor(x => x.Items)
                .Must(SpecifyEachDayOnlyOnce).WithMessage("A day can only appear one schedule item");

        }

        private bool SpecifyEachDayOnlyOnce(Schedule[] items)
        {
            var allDays = items.SelectMany(x => x.Days).ToList();
            return allDays.Count == allDays.Distinct().Count();
        }
    }
}
