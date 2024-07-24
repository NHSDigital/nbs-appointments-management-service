using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class SetUserRoleValidator : AbstractValidator<SetUserRolesRequest>
{
    public SetUserRoleValidator()
    {
        RuleFor(x => x.Scope).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Provide a valid scope - 'site:<siteId>'")
            .Matches(@"site:[A-Za-z0-9]+").WithMessage("Provide a valid scope - 'site:<siteId>'");
        RuleFor(x => x.User).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Provide a valid email address")
            .Matches(@"[\w-\.]+@([\w-]+\.)+[\w-]{2,4}").WithMessage("Provide a valid email address");
        RuleFor(x => x.Roles).NotEmpty().WithMessage("One or more roles must be provided");
        RuleForEach(x => x.Roles).NotEmpty().WithMessage("Provide at least one valid role");
    }
}
