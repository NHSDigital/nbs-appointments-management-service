using FluentValidation;

namespace CsvDataTool.Validators;

public class ApiUserImportRowValidator : AbstractValidator<ApiUserDataImportHandler.ApiUserImportRow>
{
    // Why are these called ClientIds? Should prod be in this list?
    private readonly string[] _validClientIds = ["local", "dev", "int", "stag", "pen", "perf", "prod"];

    public ApiUserImportRowValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .MustNotContainWhitespace()
            .Must(value => _validClientIds.Contains(value))
            .WithMessage(
                $"{nameof(ApiUserDataImportHandler.ApiUserImportRow.ClientId)} must be one of the following: {string.Join(", ", _validClientIds)}");

        RuleFor(x => x.ApiSigningKey)
            .NotEmpty()
            .MustNotContainWhitespace()
            .Length(88); // This is the length of existing Api Signing Keys on dev, int, & stag
    }
}
