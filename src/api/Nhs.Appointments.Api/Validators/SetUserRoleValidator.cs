using FluentValidation;
using FluentValidation.Results;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class SetUserRoleValidator : AbstractValidator<SetUserRolesRequest>
{
    public SetUserRoleValidator()
    {
        RuleFor(x => x.Scope).NotEmpty().WithMessage("Provide a valid scope - site:<siteId>");
        RuleFor(x => x.User).NotEmpty().WithMessage("Provide a valid email address");
        RuleFor(x => x.Roles).NotEmpty().WithMessage("One or more roles must be provided");
        RuleForEach(x => x.Roles).NotEmpty().WithMessage("All provided roles must be valid");
    }
}
