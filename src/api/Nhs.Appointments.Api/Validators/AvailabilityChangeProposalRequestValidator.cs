using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;
public class AvailabilityChangeProposalRequestValidator : AbstractValidator<AvailabilityChangeProposalRequest>
{
    public AvailabilityChangeProposalRequestValidator()
    {
        RuleFor(x => x.From).NotEmpty().WithMessage("From date is required.");
        RuleFor(x => x.To).NotEmpty().WithMessage("To date is required.");
        RuleFor(x => x.Site).NotEmpty().WithMessage("Site ID is required.");
        RuleFor(x => x.SessionMatcher).NotEmpty().WithMessage("Session Matcher is required.");
        RuleFor(x => x.SessionReplacement).NotEmpty().WithMessage("Session Replacement is required.");
    }
}
