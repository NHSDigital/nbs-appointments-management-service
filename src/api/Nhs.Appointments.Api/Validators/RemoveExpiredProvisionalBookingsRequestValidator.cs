using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class RemoveExpiredProvisionalBookingsRequestValidator : AbstractValidator<RemoveExpiredProvisionalBookingsRequest>
{
    public RemoveExpiredProvisionalBookingsRequestValidator()
    {
        // Validate BatchSize only if provided
        When(x => x != null && x.BatchSize.HasValue, () =>
        {
            RuleFor(x => x.BatchSize.Value)
                .GreaterThan(0)
                .WithMessage("BatchSize must be a positive integer");
        });

        // Validate DegreeOfParallelism only if provided
        When(x => x != null && x.DegreeOfParallelism.HasValue, () =>
        {
            RuleFor(x => x.DegreeOfParallelism.Value)
                .GreaterThanOrEqualTo(1)
                .WithMessage("DegreeOfParallelism must be at least 1");
        });

    }
}
