using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class QueryBookingByNhsNumberRequestValidator : AbstractValidator<QueryBookingByNhsNumberRequest>
{
    public QueryBookingByNhsNumberRequestValidator()
    {
        RuleFor(x => x.nhsNumber)
            .NotEmpty().WithMessage("Provide a valid Nhs Number");
    }
}


