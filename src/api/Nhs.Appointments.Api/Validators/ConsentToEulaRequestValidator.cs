using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class ConsentToEulaRequestValidator : AbstractValidator<ConsentToEulaRequest>
{
    public ConsentToEulaRequestValidator()
    {
        RuleFor(x => x.userId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Provide a valid email address")
            .Matches(@"[\w-\.]+@([\w-]+\.)+[\w-]{2,4}").WithMessage("Provide a valid email address");

        RuleFor(x => x.versionDate)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Provide a date in 'yyyy-MM-dd'");
    }
}
