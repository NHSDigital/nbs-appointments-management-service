using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Nhs.Appointments.Api.Models;

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
        RuleFor(x => x.DateOfBirth).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Provide a date of birth in the format 'yyyy-MM-dd'")
            .Must(x => DateOnly.TryParseExact(x.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var _)).WithMessage("Provide a date of birth in the format 'yyyy-MM-dd'")
            .DependentRules(() =>
            {
                RuleFor(x => x.BirthDate)
                    .LessThan(x => today).WithMessage("Date of birth must be in the past");
            });
    }
}

public class EmptyValidator : AbstractValidator<EmptyRequest>
{
    
}
