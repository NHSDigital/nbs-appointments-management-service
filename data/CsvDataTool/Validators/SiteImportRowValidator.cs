using FluentValidation;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace CsvDataTool.Validators;

public class SiteImportRowValidator : AbstractValidator<SiteDocument>
{
    private readonly string[] _validSiteTypes = ["Pharmacy"];

    public SiteImportRowValidator()
    {
        RuleFor(x => x.Id)
            .MustNotBeEmpty()
            .MustNotContainWhitespace()
            .IsGuid();
        RuleFor(x => x.OdsCode)
            .MustNotBeEmpty()
            .IsUppercase()
            .MinimumLength(3)
            .WithMessage("{CollectionIndex}: {PropertyName} must be at least 3 characters")
            .MaximumLength(10)
            .WithMessage("{CollectionIndex}: {PropertyName} must not exceed 10 characters")
            .MustNotContainWhitespace();
        RuleFor(x => x.Name)
            .MustNotBeEmpty();
        RuleFor(x => x.Address)
            .MustNotBeEmpty();
        RuleFor(x => x.PhoneNumber)
            .MustNotBeEmpty()
            .PhoneNumber();
        RuleFor(x => x.Longitude)
            .NotEmpty()
            .WithMessage("{CollectionIndex}: {PropertyName} must be provided")
            .GreaterThanOrEqualTo(-8.1)
            .WithMessage("{CollectionIndex}: Longitude must be greater than or equal to -8.1")
            .LessThanOrEqualTo(1.8)
            .WithMessage("{CollectionIndex}: Longitude must be less than or equal to 1.8");
        RuleFor(x => x.Latitude)
            .NotEmpty()
            .WithMessage("{CollectionIndex}: {PropertyName} must be provided")
            .GreaterThanOrEqualTo(49.8)
            .WithMessage("{CollectionIndex}: {PropertyName} must be greater than or equal to 49.8")
            .LessThanOrEqualTo(60.9)
            .WithMessage("{CollectionIndex}: {PropertyName} must be less than or equal to 60.9");
        RuleFor(x => x.IntegratedCareBoard)
            .MustNotBeEmpty()
            .IsUppercase()
            .MinimumLength(3)
            .WithMessage("{CollectionIndex}: {PropertyName} must be at least 3 characters")
            .MaximumLength(10)
            .WithMessage("{CollectionIndex}: {PropertyName} must not exceed 10 characters")
            .MustNotContainWhitespace();
        RuleFor(x => x.Region)
            .MustNotBeEmpty();
        RuleFor(x => x.Type)
            .MustNotBeEmpty()
            .Must(value => _validSiteTypes.Contains(value))
            .WithMessage("{CollectionIndex}: {PropertyName} must be Pharmacy");

        RuleFor(x => x.Accessibilities)
            .SetValidator(new AccessibilityAttributesValidator())
            .WithMessage("{CollectionIndex}: All 9 accessibility attributes must be provided");
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
                accessibilityAttributeIds.Contains("accessibility/accessible_toilet") &&
                accessibilityAttributeIds.Contains("accessibility/braille_translation_service") &&
                accessibilityAttributeIds.Contains("accessibility/disabled_car_parking") &&
                accessibilityAttributeIds.Contains("accessibility/car_parking") &&
                accessibilityAttributeIds.Contains("accessibility/induction_loop") &&
                accessibilityAttributeIds.Contains("accessibility/sign_language_service") &&
                accessibilityAttributeIds.Contains("accessibility/step_free_access") &&
                accessibilityAttributeIds.Contains("accessibility/text_relay") &&
                accessibilityAttributeIds.Contains("accessibility/wheelchair_access");
        });

        RuleForEach(x => x).Must(y => bool.TryParse(y.Value, out _));
    }
}
