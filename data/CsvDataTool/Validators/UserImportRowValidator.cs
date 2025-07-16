using FluentValidation;

namespace CsvDataTool.Validators;

public class UserImportRowValidator : AbstractValidator<UserImportRow>
{
    public UserImportRowValidator()
    {
        RuleFor(x => x.UserId)
            .MustNotBeEmpty()
            .EmailAddress()
            .WithMessage("{CollectionIndex}: {PropertyName} must be a valid email address")
            .IsLowercase()
            .MustNotContainWhitespace();
        RuleFor(x => x.SiteId)
            .MustNotBeEmpty()
            .MustNotContainWhitespace()
            .IsGuid();
    }
}
