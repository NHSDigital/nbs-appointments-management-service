using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class CancelBookingRequestValidator : AbstractValidator<CancelBookingRequest>
{
    public CancelBookingRequestValidator() 
    {
        RuleFor(x => x.bookingReference)
            .NotEmpty().WithMessage("Provide a booking reference");
        RuleFor(x => x.site)
            .NotEmpty().WithMessage("Provide a site");
    }
}
