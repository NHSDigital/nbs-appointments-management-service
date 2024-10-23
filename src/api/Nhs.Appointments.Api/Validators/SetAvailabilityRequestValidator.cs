using FluentValidation;
using Nhs.Appointments.Api.Availability;
using System;
using System.Globalization;

namespace Nhs.Appointments.Api.Validators
{
    public class SetAvailabilityRequestValidator : AbstractValidator<SetAvailabilityRequest>
    {
        public SetAvailabilityRequestValidator()
        {
            RuleFor(x => x.Date).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(x => DateOnly.TryParseExact(x.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var _))
                .WithMessage("Provide a date in the format 'yyyy-MM-dd'");

            RuleFor(x => x.Site)
                .NotEmpty()
                .WithMessage("Provide a valid site.");

            RuleFor(x => x.Sessions)
                .NotEmpty()
                .WithMessage("Provide one or more valid sessions");

            RuleForEach(x => x.Sessions)
                .SetValidator(new SessionValidator());
        }
    }
}
