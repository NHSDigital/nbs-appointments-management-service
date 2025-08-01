using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Models;
public record SetSiteStatusRequest(string site, SiteStatus status);
