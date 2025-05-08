using FluentValidation;
using FluentValidation.Results;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class ProposePotentialUserRequestValidator : AbstractValidator<ProposePotentialUserRequest>
{
    public ProposePotentialUserRequestValidator()
    {
        RuleFor(request => request.SiteId)
            .NotEmpty()
            .WithMessage("Provide a valid site.");
        RuleFor(request => request.UserId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Provide an email address.")
            .Matches(@"[\w-\.]+@([\w-]+\.)+[\w-]{2,4}")
            .WithMessage("Provide a valid email address.");
    }

    protected override bool PreValidate(ValidationContext<ProposePotentialUserRequest> requestBody,
        ValidationResult result)
    {
        if (requestBody.InstanceToValidate == null)
        {
            result.Errors.Add(new ValidationFailure("", "A request body must be provided"));
            return false;
        }

        return true;
    }
}
