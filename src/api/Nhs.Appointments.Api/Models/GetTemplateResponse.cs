using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Models;

public record GetTemplateResponse
{
    public WeekTemplate[] Templates { get; set; }
}
