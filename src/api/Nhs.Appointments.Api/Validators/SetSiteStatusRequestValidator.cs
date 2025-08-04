using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;
public class SetSiteStatusRequestValidator : AbstractValidator<SetSiteStatusRequest>
{
    public SetSiteStatusRequestValidator()
    {
        RuleFor(x => x.Site)
            .NotEmpty()
            .WithMessage("Provide a valid site.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Provide a valid site status.");
    }
}
