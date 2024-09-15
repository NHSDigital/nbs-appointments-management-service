using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class SetSiteAttributeValuesValidator : AbstractValidator<SetSiteAttributesRequest>
{
    public SetSiteAttributeValuesValidator()
    {
        RuleFor(x => x.Site)
            .NotEmpty().WithMessage("Provide a valid site");
        RuleFor(x => x.AttributeValues)
            .NotEmpty().WithMessage("One or more attribute values must be provided");
        RuleForEach(x => x.AttributeValues)
            .NotEmpty().WithMessage("Provide at least one valid attribute and value");
    }
}
