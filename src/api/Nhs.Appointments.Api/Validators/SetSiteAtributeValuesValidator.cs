using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class SetSiteAttributeValuesValidator : AbstractValidator<SetSiteAttributesRequest>
{
    public SetSiteAttributeValuesValidator()
    {
        RuleFor(x => x.Site)
            .NotEmpty()
            .WithMessage("Provide a valid site");
        RuleFor(x => x.Scope)
            .NotEmpty()
            .WithMessage("Provide a valid scope");
        RuleFor(x => x.AttributeValues)
            .NotEmpty()
            .WithMessage("Attribute values must be provided");
        RuleForEach(x => x.AttributeValues)
            .SetValidator(new AttributeValueValidator());
    }
}
