using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;
public class SessionOrWildcardValidator : AbstractValidator<SessionOrWildcard>
{
    public SessionOrWildcardValidator()
    {
        When(x => !x.IsWildcard, () =>
        {
            RuleFor(x => x.Session)
                .NotNull()
                .WithMessage("Session must be provided when not using '*'.")
                .SetValidator(new SessionValidator());  
        });

        When(x => x.IsWildcard, () =>
        {
            RuleFor(x => x.Session)
                .Null()
                .WithMessage("Wildcard cannot include a session object.");
        });
    }
}
