using FluentValidation;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Validators;

public class SetBookingStatusValidator : AbstractValidator<SetBookingStatusRequest>
{
    public SetBookingStatusValidator()
    {
        RuleFor(x => x.bookingReference)
            .NotEmpty().WithMessage("Provide a booking reference");
        RuleFor(x => x.status)
            .NotEmpty().WithMessage("Provide a status");
    }
}