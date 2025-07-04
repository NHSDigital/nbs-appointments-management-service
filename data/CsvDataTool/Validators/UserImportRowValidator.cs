using FluentValidation;

namespace CsvDataTool.Validators;

public class UserImportRowValidator : AbstractValidator<UserDataImportHandler.UserImportRow>
{
    public UserImportRowValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .EmailAddress()
            .IsLowercase()
            .MustNotContainWhitespace();
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .MustNotContainWhitespace()
            .IsGuid();
    }
}
