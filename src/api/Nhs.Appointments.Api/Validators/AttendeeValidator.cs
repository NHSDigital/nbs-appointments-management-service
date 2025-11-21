using FluentValidation;
using Nhs.Appointments.Core.Availability;

namespace Nhs.Appointments.Api.Validators;
public class AttendeeValidator : AbstractValidator<Attendee>
{
    public AttendeeValidator()
    {
        RuleFor(x => x.Services)
            .NotEmpty()
            .WithMessage("Provide at least one servivce.");
    }
}
