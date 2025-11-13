using Nhs.Appointments.Core.Sites;

namespace Nhs.Appointments.Api.Models;
public record SetSiteStatusRequest(string Site, SiteStatus Status);
