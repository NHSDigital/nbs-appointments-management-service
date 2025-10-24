using FluentValidation;
using Nhs.Appointments.Api.Models;
using System;

namespace Nhs.Appointments.Api.Validators;
public class QuerySitesRequestValidator : AbstractValidator<QuerySitesRequest>
{
    public QuerySitesRequestValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.Filters)
            .NotEmpty()
            .WithMessage("Provide at least one site query filter.");
        RuleForEach(x => x.Filters)
            .SetValidator(new SiteFilterValidator(timeProvider));
    }
}
