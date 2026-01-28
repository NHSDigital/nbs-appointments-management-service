using FluentValidation;
using Nhs.Appointments.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Validators;

public class GetSiteUsersReportRequestValidator : AbstractValidator<GetSiteUsersReportRequest>
{
    public GetSiteUsersReportRequestValidator()
    {
        RuleFor(x => x.Site)
            .NotEmpty()
            .WithMessage("Site is required.");
    }
}
