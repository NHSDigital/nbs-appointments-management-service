using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;
public class BulkImportRequestValidator : AbstractValidator<BulkImportRequest>
{
    public BulkImportRequestValidator()
    {
        RuleFor(x => x.File)
            .NotEmpty()
            .WithMessage("Provide a csv file.");

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Provide an import type");
    }
}
