using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class RemoveUserValidator : AbstractValidator<RemoveUserRequest>
{
    public RemoveUserValidator()
    {
        RuleFor(x => x.Site).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Provide a site ID");
        RuleFor(x => x.User).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Provide a valid email address")
            .Matches(@"[\w-\.]+@([\w-]+\.)+[\w-]{2,4}").WithMessage("Provide a valid email address");
    }
}
