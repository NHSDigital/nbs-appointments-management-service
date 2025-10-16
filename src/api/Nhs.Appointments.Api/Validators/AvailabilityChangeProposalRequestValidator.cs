using FluentValidation;
using Nhs.Appointments.Api.Models;
using System;

namespace Nhs.Appointments.Api.Validators;
public class AvailabilityChangeProposalRequestValidator
    : AbstractValidator<AvailabilityChangeProposalRequest>
{
    public AvailabilityChangeProposalRequestValidator(TimeProvider timeProvider)
    {
        Include(new BaseSessionRequestValidator(timeProvider));
    }
}
