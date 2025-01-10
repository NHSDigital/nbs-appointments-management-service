using System;
using FluentValidation;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Validators
{
    public class SetAvailabilityRequestValidator : AbstractValidator<SetAvailabilityRequest>
    {
        public SetAvailabilityRequestValidator(TimeProvider timeProvider)
        {
            RuleFor(x => x.Date)
                .GreaterThanOrEqualTo(DateOnly.Parse(timeProvider.GetUtcNow().AddDays(1).ToString("yyyy-MM-dd")))
                .WithMessage("Date must be at least 1 day in the future")
                .LessThanOrEqualTo(DateOnly.Parse(timeProvider.GetUtcNow().AddYears(1).ToString("yyyy-MM-dd")))
                .WithMessage("Date cannot be later than 1 year from now");

            RuleFor(x => x.Site)
                .NotEmpty()
                .WithMessage("Provide a valid site.");

            RuleFor(x => x.Sessions)
                .NotEmpty()
                .WithMessage("Provide one or more valid sessions");

            RuleForEach(x => x.Sessions)
                .SetValidator(new SessionValidator());

            RuleFor(x => x.SessionToEdit)
                .NotNull()
                .When(x => x.Mode == ApplyAvailabilityMode.Edit)
                .WithMessage("A session to edit must be provided when mode is set to Edit");

            RuleFor(x => x.Sessions)
                .Must(x => x.Length == 1)
                .When(x => x.Mode == ApplyAvailabilityMode.Edit)
                .WithMessage("Only one edited session can be provided when mode is set to Edit");
        }
    }
}
