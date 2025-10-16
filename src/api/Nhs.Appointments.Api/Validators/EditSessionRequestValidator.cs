using FluentValidation;
using Nhs.Appointments.Api.Models;
using System;

namespace Nhs.Appointments.Api.Validators;
public class EditSessionRequestValidator
    : AbstractValidator<EditSessionRequest>
{
    public EditSessionRequestValidator(TimeProvider timeProvider)
    {
        Include(new BaseSessionRequestValidator(timeProvider));
    }
}
