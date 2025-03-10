using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class FeatureFlagEnabledRequestValidator : AbstractValidator<FeatureFlagEnabledRequest>
{ 
    public FeatureFlagEnabledRequestValidator()
    {        
        RuleFor(x => x.Flag).NotEmpty().WithMessage("Provide a valid flag");        
    }
}
