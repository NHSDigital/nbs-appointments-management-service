using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class ConsentToEulaRequestValidator : AbstractValidator<ConsentToEulaRequest>
{
    public ConsentToEulaRequestValidator()
    {
    }
}
