using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class FeatureFlagRequestValidator : AbstractValidator<FeatureFlagRequest>
{ 
    public FeatureFlagRequestValidator()
    {        
        RuleFor(x => x.Flag).NotEmpty().WithMessage("Provide a valid flag");        
    }
}

public class SetFeatureFlagOverrideRequestValidator : AbstractValidator<SetFeatureFlagOverrideRequest>
{ 
    public SetFeatureFlagOverrideRequestValidator()
    {        
        RuleFor(x => x.Flag).NotEmpty().WithMessage("Provide a valid flag");    
    }
}
