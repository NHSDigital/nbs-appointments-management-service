using System;
using System.Globalization;
using FluentValidation;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Validators;

public class AttributeValueValidator : AbstractValidator<AttributeValue>
{
    public AttributeValueValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Provide a valid attribute id");
        RuleFor(x => x.Value)
            .Must(value => value is "true" or "false")
            .WithMessage("An attribute value must be provided (value must be true or false)");
    }
}
