using System;
using System.Globalization;
using FluentValidation;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Validators;

public class AttendeeDetailsValidator : AbstractValidator<AttendeeDetails>
{
    public AttendeeDetailsValidator()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        
        RuleFor(x => x.NhsNumber).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Provide a valid Nhs number")
            .Matches(@"^[0-9]+$").WithMessage("Provide a valid Nhs number")
            .Length(10).WithMessage("Provide a valid Nhs number");
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Provide a first name");
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Provide a last name");
        RuleFor(x => x.DateOfBirth)
            .LessThan(x => today).WithMessage("Date of birth must be in the past");            
    }
}
