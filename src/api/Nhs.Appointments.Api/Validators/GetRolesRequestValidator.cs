using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class GetRolesRequestValidator : AbstractValidator<GetRolesRequest>
{
    public GetRolesRequestValidator()
    {
        RuleFor(x => x.tag)
            .NotEmpty().WithMessage("Provide a tag");
    }
}