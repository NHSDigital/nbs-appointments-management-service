using FluentValidation;

namespace CsvDataTool.Validators;

public static class CustomValidators
{
    public static IRuleBuilderOptions<T, string>
        MustNotContainWhitespace<T>(this IRuleBuilder<T, string> ruleBuilder) => ruleBuilder
        .Must(value => !value.ToCharArray().Any(char.IsWhiteSpace))
        .WithMessage("{CollectionIndex}: {PropertyName} must not contain whitespace");

    public static IRuleBuilderOptions<T, string>
        NoLeadingOrTrailingWhitespace<T>(this IRuleBuilder<T, string> ruleBuilder) => ruleBuilder
        .Must(value => value.Trim() == value)
        .WithMessage("{CollectionIndex}: {PropertyName} must not begin or end with whitespace");

    public static IRuleBuilderOptions<T, string>
        IsLowercase<T>(this IRuleBuilder<T, string> ruleBuilder) => ruleBuilder
        .Must(value => value.Equals(value.ToLowerInvariant()))
        .WithMessage("{CollectionIndex}: {PropertyName} must be lowercase");

    public static IRuleBuilderOptions<T, string>
        IsUppercase<T>(this IRuleBuilder<T, string> ruleBuilder) => ruleBuilder
        .Must(value => value.Equals(value.ToUpperInvariant()))
        .WithMessage("{CollectionIndex}: {PropertyName} must be uppercase");

    public static IRuleBuilderOptions<T, string>
        IsGuid<T>(this IRuleBuilder<T, string> ruleBuilder) => ruleBuilder
        .Must(value => Guid.TryParse(value, out var fooGuid))
        .WithMessage("{CollectionIndex}: {PropertyName} must be a valid GUID");

    public static IRuleBuilderOptions<T, string>
        PhoneNumber<T>(this IRuleBuilder<T, string> ruleBuilder) => ruleBuilder
        .Matches(@"^\+?[\d\s]{7,}$")
        .WithMessage("{CollectionIndex}: {PropertyName} must be a valid phone number");

    public static IRuleBuilderOptions<T, string>
        MustNotBeEmpty<T>(this IRuleBuilder<T, string> ruleBuilder) => ruleBuilder
        .NotEmpty()
        .WithMessage("{CollectionIndex}: {PropertyName} must be provided");
}
