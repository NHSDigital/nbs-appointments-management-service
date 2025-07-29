using System;

namespace Nhs.Appointments.Api.Models;

public record SiteReportRequest(DateOnly StartDate , DateOnly EndDate);
