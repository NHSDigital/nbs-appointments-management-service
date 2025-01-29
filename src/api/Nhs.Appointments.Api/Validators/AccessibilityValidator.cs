using FluentValidation;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Validators;

public class AccessibilityValidator : AbstractValidator<Accessibility>
{
    public AccessibilityValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Provide a valid attribute id");
    }
}
