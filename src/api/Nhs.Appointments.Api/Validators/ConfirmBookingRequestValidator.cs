using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class ConfirmBookingRequestValidator : AbstractValidator<ConfirmBookingRequest>
{
    
    public ConfirmBookingRequestValidator()
    {
        RuleFor(x => x.bookingReference)
            .NotEmpty().WithMessage("Provide a valid booking reference");
    }
}

