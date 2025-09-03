using FluentValidation;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Validators;

public class QueryBookingsRequestValidator : AbstractValidator<QueryBookingsRequest>
{
    public QueryBookingsRequestValidator() 
    {
        RuleFor(x => x.site)
            .NotEmpty()
            .WithMessage("Provide a valid site");
        RuleFor(x => x.from)
            .LessThanOrEqualTo(x => x.to)
            .WithMessage("Provide a valid date range");
        RuleForEach(x => x.statuses)
            .IsEnumName(typeof(AppointmentStatus))
            .WithMessage("Provide valid appointment statuses");
        RuleFor(x => x.cancellationReason)
            .IsEnumName(typeof(CancellationReason))
            .WithMessage("Provide a valid cancellation reason");
        RuleForEach(x => x.cancellationNotificationStatuses)
            .IsEnumName(typeof(CancellationNotificationStatus))
            .WithMessage("Provide valid cancellation notification statuses");
    }
}

