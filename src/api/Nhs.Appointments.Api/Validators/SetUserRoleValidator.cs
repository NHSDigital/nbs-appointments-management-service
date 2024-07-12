using FluentValidation;
using Nhs.Appointments.Api.Models;
using System;
using System.Linq;

namespace Nhs.Appointments.Api.Validators
{
    public class SetUserRoleValidator : AbstractValidator<SetUserRolesRequest>
    {
        public SetUserRoleValidator()
        {
            RuleFor(x => x.Scope).NotEmpty().WithMessage("Provide a valid scope - site:<siteId>");
        }
    }
}