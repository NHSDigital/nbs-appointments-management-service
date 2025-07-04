using FluentValidation;

namespace CsvDataTool.Validators;

public static class CustomValidators
{
    public static IRuleBuilderOptions<T, string>
        MustNotContainWhitespace<T>(this IRuleBuilder<T, string> ruleBuilder) => ruleBuilder
        .Must(value => !value.ToCharArray().Any(char.IsWhiteSpace))
        .WithMessage("{PropertyName} must not contain whitespace");

    public static IRuleBuilderOptions<T, string>
        NoLeadingOrTrailingWhitespace<T>(this IRuleBuilder<T, string> ruleBuilder) => ruleBuilder
        .Must(value => !(value.StartsWith(' ') || value.EndsWith(' ')))
        .WithMessage("{PropertyName} must not begin or end with whitespace");

    public static IRuleBuilderOptions<T, string>
        IsLowercase<T>(this IRuleBuilder<T, string> ruleBuilder) => ruleBuilder
        .Must(value => value.Equals(value.ToLowerInvariant()))
        .WithMessage("{PropertyName} must be lowercase");

    public static IRuleBuilderOptions<T, string>
        IsUppercase<T>(this IRuleBuilder<T, string> ruleBuilder) => ruleBuilder
        .Must(value => value.Equals(value.ToUpperInvariant()))
        .WithMessage("{PropertyName} must be uppercase");

    public static IRuleBuilderOptions<T, string>
        IsGuid<T>(this IRuleBuilder<T, string> ruleBuilder) => ruleBuilder
        .Must(value => Guid.TryParse(value, out var fooGuid))
        .WithMessage("{PropertyName} must be a valid GUID");

    public static IRuleBuilderOptions<T, string>
        PhoneNumber<T>(this IRuleBuilder<T, string> ruleBuilder) => ruleBuilder
        .Matches(@"^\+?[\d\s]{7,}$")
        .WithMessage("{PropertyName} must be a phone number");
}
