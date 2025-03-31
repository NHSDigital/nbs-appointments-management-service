using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class GetFeatureFlagRequestValidator : AbstractValidator<GetFeatureFlagRequest>
{ 
    public GetFeatureFlagRequestValidator()
    {        
        RuleFor(x => x.Flag).NotEmpty().WithMessage("Provide a valid flag");        
    }
}

public class SetLocalFeatureFlagOverrideRequestValidator : AbstractValidator<SetLocalFeatureFlagOverrideRequest>
{ 
    public SetLocalFeatureFlagOverrideRequestValidator()
    {        
        RuleFor(x => x.Flag).NotEmpty().WithMessage("Provide a valid flag");    
    }
}
