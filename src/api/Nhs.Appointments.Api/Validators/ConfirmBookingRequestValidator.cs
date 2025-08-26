using System.Linq;
using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class ConfirmBookingRequestValidator : AbstractValidator<ConfirmBookingRequest>
{
    
    public ConfirmBookingRequestValidator()
    {
        RuleFor(x => x.bookingReference)
            .NotEmpty().WithMessage("Provide a valid booking reference");
        When(x => x.relatedBookings is not null, () =>
        {
            RuleForEach(x => x.relatedBookings)
                .NotEmpty()
                .WithMessage("Each related booking must be a valid booking reference")
                .NotEqual(x => x.bookingReference)
                .WithMessage("Related bookings must not include the primary booking reference")
                .NotEqual(x => x.bookingToReschedule)
                .WithMessage("Related bookings must not include the booking to reschedule");
            RuleFor(x => x.relatedBookings)
                .Must(x => x.Distinct().Count() == x.Length)
                .WithMessage("Related bookings must not include the same booking more than once");
        });
        When(x => x.bookingToReschedule is not null, () =>
        {
            RuleFor(x => x.bookingToReschedule)
                .NotEqual(x => x.bookingReference)
                .WithMessage("The booking to reschedule cannot be the primary booking reference");
        });
    }
}

