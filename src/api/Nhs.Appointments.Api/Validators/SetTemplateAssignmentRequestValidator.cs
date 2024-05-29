using FluentValidation;
using Nhs.Appointments.Api.Models;
using System;
using System.Linq;

namespace Nhs.Appointments.Api.Validators
{
    public class SetTemplateAssignmentRequestValidator : AbstractValidator<SetTemplateAssignmentRequest> 
    { 
        public SetTemplateAssignmentRequestValidator() 
        {
            RuleFor(x => x.Site)
                .NotEmpty().WithMessage("Provide a site");

            RuleFor(x => x.Assignments).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Assignments must be specified")
                .DependentRules(() =>
                {
                    RuleFor(x => x.Assignments)
                        .Must(HaveValidTemplateIds).WithMessage("All assignments must have a valid template id");

                    RuleFor(x => x.Assignments).Cascade(CascadeMode.Stop)
                       .Must(HaveValidDates).WithMessage("Assignments must have valid dates provided in the format 'yyyy-MM-dd'")
                       .DependentRules(() =>
                           RuleFor(x => x.Assignments).Cascade(CascadeMode.Stop)
                               .Must(NotHaveInvalidDateRanges).WithMessage("All assignments must have valid date ranges")
                               .DependentRules(() =>
                                   RuleFor(x => x.Assignments)
                                       .Must(NotHaveOverlappingAssignments).WithMessage("Assignments cannot contain overlapping date periods")));
                });
        }        

        private bool HaveValidDates(TemplateAssignment[] assignments)
        {
            return assignments.All(x => DateOnly.TryParseExact(x.From, "yyyy-MM-dd", out var _) && DateOnly.TryParseExact(x.Until, "yyyy-MM-dd", out var _));
        }

        private bool NotHaveOverlappingAssignments(TemplateAssignment[] assignments)
        {
            var zeroTime = new TimeOnly(0, 0);
            var timePeriods = assignments.Select(a => new Core.TimePeriod(a.FromDate.ToDateTime(zeroTime), a.UntilDate.ToDateTime(zeroTime))).ToList();
            foreach(var timePeriod in timePeriods) 
            {
                if (timePeriods.Any(tp => tp != timePeriod && tp.Overlaps(timePeriod)))
                    return false;
            }

            return true;
        }

        private bool NotHaveInvalidDateRanges(TemplateAssignment[] assignments)
        {
            return assignments.All(a => a.FromDate <= a.UntilDate);            
        }

        private bool HaveValidTemplateIds(TemplateAssignment[] assignments)
        {
            return assignments.All(a => !string.IsNullOrEmpty(a.TemplateId));
        }
    }
}
