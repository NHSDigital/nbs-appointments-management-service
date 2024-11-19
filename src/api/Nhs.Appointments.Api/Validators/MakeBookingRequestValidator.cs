﻿using System;
using System.Globalization;
using FluentValidation;
using FluentValidation.Results;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class MakeBookingRequestValidator : AbstractValidator<MakeBookingRequest>
{
    public MakeBookingRequestValidator()
    {
        RuleFor(x => x.Site)
            .NotEmpty().WithMessage("A site identifier must be provided");
        RuleFor(x => x.Duration)
            .InclusiveBetween(1, 300).WithMessage("Appointment duration must be between 1 and 300");
        RuleFor(x => x.From).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Provide a date and time in the format 'yyyy-MM-dd HH:mm'")
            .Must(x => DateTime.TryParseExact(x, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var _)).WithMessage("Provide a date and time in the format 'yyyy-MM-dd HH:mm'");
        RuleFor(x => x.Service)
            .NotEmpty().WithMessage("Provide a valid service");        
        RuleFor(x => x.AttendeeDetails)
            .NotEmpty().WithMessage("Provide attendee details")
            .SetValidator(new AttendeeDetailsValidator());
        RuleFor(x => x.ContactDetails)
            .NotEmpty().When(x => !x.Provisional).WithMessage("Provide contact details");
        RuleFor(x => x.ContactDetails)
            .Empty().When(x => x.Provisional).WithMessage("A provisional booking should not include contact details");
    }
    
    protected override bool PreValidate(ValidationContext<MakeBookingRequest> requestBody, ValidationResult result) 
    {
        if (requestBody.InstanceToValidate == null) 
        {
            result.Errors.Add(new ValidationFailure("", "A request body must be provided"));
            return false;
        }
        return true;
    }
}
