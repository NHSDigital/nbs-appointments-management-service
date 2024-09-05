using FluentValidation;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Validators;

public class GetSitesRequestValidator : AbstractValidator<GetSitesRequest>
{
    public GetSitesRequestValidator()
    {
        RuleFor(x => x.longitude)
            .NotEmpty().WithMessage("Provide a longitude value");
        RuleFor(x => x.latitude)
            .NotEmpty().WithMessage("Provide a latitude value");
        RuleFor(x => x.searchRadius)
            .NotEmpty().WithMessage("Provide a search radius");
        RuleFor(x => x.maxiumRecords)
            .NotEmpty().WithMessage("Provide a number of maximum records");
    }
}