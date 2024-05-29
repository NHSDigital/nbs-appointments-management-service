using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class QueryBookingsRequestValidator : AbstractValidator<QueryBookingsRequest>
{
    public QueryBookingsRequestValidator() 
    {
        RuleFor(x => x.site)
            .NotEmpty().WithMessage("Provide a valid site");
        RuleFor(x => x.from)
            .LessThanOrEqualTo(x => x.to).WithMessage("Provide a valid date range");
    }
}

