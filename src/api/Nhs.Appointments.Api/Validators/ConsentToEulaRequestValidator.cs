using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class ConsentToEulaRequestValidator : AbstractValidator<ConsentToEulaRequest>
{
    public ConsentToEulaRequestValidator()
    {
        RuleFor(x => x.versionDate)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Provide a date in 'yyyy-MM-dd'");
    }
}
