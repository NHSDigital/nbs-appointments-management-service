using FluentValidation;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Validators;

public class AttributeValueValidator : AbstractValidator<AttributeValue>
{
    public AttributeValueValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Provide a valid attribute id");
    }
}
