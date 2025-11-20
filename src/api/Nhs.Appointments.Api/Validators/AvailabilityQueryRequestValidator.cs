using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;
public class AvailabilityQueryRequestValidator : AbstractValidator<AvailabilityQueryRequest>
{
    public AvailabilityQueryRequestValidator()
    {
        RuleFor(x => x.Sites)
            .NotEmpty()
            .WithMessage("Provide a list of sites.");
        // Add custom attendee model validator
        RuleFor(x => x.Attendees)
            .NotEmpty()
            .WithMessage("Provide a list of attendees.");
        RuleFor(x => x.From)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Provide a date in 'yyyy-MM-dd'")
            .LessThanOrEqualTo(x => x.Until)
            .WithMessage("'from' date must be before 'until' date");
        RuleFor(x => x.Until)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Provide an until date in 'yyyy-MM-dd'")
            .GreaterThanOrEqualTo(x => x.From)
            .WithMessage("'until' date must be after 'from' date");
    }
}
