using System;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;

namespace Nhs.Appointments.Api.Availability;

public class QueryAvailabilityRequestValidator : AbstractValidator<QueryAvailabilityRequest> 
{
    public QueryAvailabilityRequestValidator()
    {
        var validQueryTypeValues = new[] { QueryType.Days, QueryType.Hours, QueryType.Slots };
        
        RuleFor(x => x.Sites)
            .NotEmpty().WithMessage("One or more site identifiers must be provided");
        RuleForEach(x => x.Sites)
            .NotEmpty().WithMessage("All provided site identifiers must be valid");
        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.Until).WithMessage("From date must be on or before Until date");        
        RuleFor(x => x.Service)
            .NotEmpty().WithMessage("Provide a valid string");
        RuleFor(x => x.QueryType)
            .Must(queryType => validQueryTypeValues.Contains(queryType)).WithMessage($"Value must be one of: {String.Join(", ", validQueryTypeValues)}");
    }
    
    protected override bool PreValidate(ValidationContext<QueryAvailabilityRequest> requestBody, ValidationResult result) 
    {
        if (requestBody.InstanceToValidate == null) 
        {
            result.Errors.Add(new ValidationFailure("", "A request body must be provided"));
            return false;
        }
        return true;
    }
}