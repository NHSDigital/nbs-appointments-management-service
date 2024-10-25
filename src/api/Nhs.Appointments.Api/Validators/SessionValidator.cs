using FluentValidation;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Validators;

public class SessionValidator : AbstractValidator<Session>
{
    public SessionValidator()
    {
        RuleFor(x => x.From).Cascade(CascadeMode.Stop)
            .LessThanOrEqualTo(x => x.Until)
            .WithMessage("'until' time must be after 'from' time")
            .DependentRules(
                () =>
                {
                    RuleFor(x => x.Until).Cascade(CascadeMode.Stop)
                        .GreaterThanOrEqualTo(x => x.From.AddMinutes(x.SlotLength))
                        .WithMessage("At least one slot must be available");
                });
        RuleFor(x => x.Capacity).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .GreaterThanOrEqualTo(1)
            .Configure(rule => rule.MessageBuilder = _ => "'capacity' must be greater than 0");
        RuleFor(x => x.SlotLength).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .GreaterThanOrEqualTo(1)
            .Configure(rule => rule.MessageBuilder = _ => "'slotLength' must be greater than 0");
        RuleFor(x => x.Services)
            .NotEmpty()
            .WithMessage("'services' cannot be empty");
        RuleForEach(x => x.Services)
            .NotEmpty()
            .WithMessage("Provide at least one service value");
    }
}
