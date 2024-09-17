using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class ContactDetailsValidator : AbstractValidator<ContactDetails>
{
    public ContactDetailsValidator()
    {
        RuleFor(m => m.PhoneNumber).NotEmpty().When(m => string.IsNullOrEmpty(m.Email)).WithMessage("Provide at least one method of contact");
        RuleFor(m => m.PhoneNumber).NotEmpty().When(m => m.PhoneContactConsent).WithMessage("Provide a mobile telephone number");
        RuleFor(m => m.Email).NotEmpty().When(m => string.IsNullOrEmpty(m.PhoneNumber)).WithMessage("Provide at least one method of contact");
        RuleFor(m => m.Email).NotEmpty().When(m =>m.EmailContactConsent).WithMessage("Provide an email address");
        RuleFor(x => x.Email).EmailAddress().When(m => !string.IsNullOrEmpty(m.Email)).WithMessage("Provide a valid email address");
    }
}
