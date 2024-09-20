using FluentValidation;
using FluentValidation.Results;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class RemoveUserRequestValidator : AbstractValidator<RemoveUserRequest>
{
    public RemoveUserRequestValidator()
    {
        RuleFor(x => x.Site).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Provide a site ID");
        RuleFor(x => x.User).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Provide a valid email address")
            .Matches(@"[\w-\.]+@([\w-]+\.)+[\w-]{2,4}").WithMessage("Provide a valid email address");
    }

    protected override bool PreValidate(ValidationContext<RemoveUserRequest> requestBody, ValidationResult result)
    {
        if (requestBody.InstanceToValidate == null)
        {
            result.Errors.Add(new ValidationFailure("", "A request body must be provided"));
            return false;
        }
        return true;
    }
}
