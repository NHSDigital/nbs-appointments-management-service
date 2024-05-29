using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Models;

public record GetSiteConfigurationResponse(Site Site, SiteConfiguration SiteConfiguration);