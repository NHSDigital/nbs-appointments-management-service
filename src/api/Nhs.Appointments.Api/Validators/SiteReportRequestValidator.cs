using FluentValidation;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Validators;

public class SiteReportRequestValidator : AbstractValidator<SiteReportRequest>
{
    public SiteReportRequestValidator()
    {
        RuleFor(x => x.StartDate).LessThan(x => x.EndDate)
            .WithMessage("Start Date should be before End Date");
    }
}
