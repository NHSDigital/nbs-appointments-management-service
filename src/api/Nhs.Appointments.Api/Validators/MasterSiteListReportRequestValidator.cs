using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class MasterSiteListReportRequestValidator : AbstractValidator<EmptyRequest>
{
    public MasterSiteListReportRequestValidator(){}
}
