using FluentValidation;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace CsvDataTool.Validators;

public class SiteDocumentValidator : AbstractValidator<SiteDocument>
{
    private readonly string[] _validSiteTypes = ["Pharmacy"];

    public SiteDocumentValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .MustNotContainWhitespace()
            .IsGuid();
        RuleFor(x => x.OdsCode)
            .NotEmpty()
            .IsUppercase()
            .MinimumLength(3)
            .MaximumLength(10)
            .MustNotContainWhitespace();
        RuleFor(x => x.Name)
            .NotEmpty();
        RuleFor(x => x.Address)
            .NotEmpty();
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .PhoneNumber();
        RuleFor(x => x.Longitude)
            .NotEmpty()
            .GreaterThanOrEqualTo(-8.1)
            .WithMessage("Longitude must be greater than or equal to -8.1")
            .LessThanOrEqualTo(1.8)
            .WithMessage("Longitude must be less than or equal to 1.8");
        RuleFor(x => x.Latitude)
            .NotEmpty()
            .GreaterThanOrEqualTo(49.8)
            .WithMessage("Latitude must be greater than or equal to 49.8")
            .LessThanOrEqualTo(60.9)
            .WithMessage("Latitude must be less than or equal to 60.9");
        RuleFor(x => x.IntegratedCareBoard)
            .NotEmpty()
            .IsUppercase()
            .MinimumLength(3)
            .MaximumLength(10)
            .MustNotContainWhitespace();
        RuleFor(x => x.Region)
            .NotEmpty();
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(value => _validSiteTypes.Contains(value));
        RuleFor(x => x.Accessibilities)
            .SetValidator(new AccessibilityAttributesValidator());
    }
}

public class AccessibilityAttributesValidator : AbstractValidator<Accessibility[]>
{
    public AccessibilityAttributesValidator()
    {
        RuleFor(x => x.Length).Equal(9);
        RuleFor(x => x).Must(x =>
        {
            var accessibilityAttributeIds = x.Select(a => a.Id).ToArray();
            return
                accessibilityAttributeIds.Contains("accessible_toilet") &&
                accessibilityAttributeIds.Contains("braille_translation_service") &&
                accessibilityAttributeIds.Contains("disabled_car_parking") &&
                accessibilityAttributeIds.Contains("car_parking") &&
                accessibilityAttributeIds.Contains("induction_loop") &&
                accessibilityAttributeIds.Contains("sign_language_service") &&
                accessibilityAttributeIds.Contains("step_free_access") &&
                accessibilityAttributeIds.Contains("text_relay") &&
                accessibilityAttributeIds.Contains("wheelchair_access");
        });

        RuleForEach(x => x).Must(y => bool.TryParse(y.Value, out _));
    }
}
