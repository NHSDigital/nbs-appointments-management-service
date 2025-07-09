using FluentValidation;

namespace CsvDataTool.Validators;

public class ApiUserImportRowValidator : AbstractValidator<ApiUserImportRow>
{
    // Why are these called ClientIds? Should prod be in this list?
    private readonly string[] _validClientIds = ["local", "dev", "int", "stag", "pen", "perf", "prod"];

    public ApiUserImportRowValidator()
    {
        RuleFor(x => x.ClientId)
            .MustNotBeEmpty()
            .MustNotContainWhitespace()
            .Must(value => _validClientIds.Contains(value))
            .WithMessage(
                $"{{CollectionIndex}}: {{PropertyName}} must be one of the following: {string.Join(", ", _validClientIds)}");

        RuleFor(x => x.ApiSigningKey)
            .MustNotBeEmpty()
            .MustNotContainWhitespace()
            .Length(88) // This is the length of existing Api Signing Keys on dev, int, & stag
            .WithMessage("{{CollectionIndex}}: {{PropertyName}} must be 88 characters long");
    }
}
