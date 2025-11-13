using System.Collections.Generic;
using Nhs.Appointments.Core.Sites;

namespace Nhs.Appointments.Api.Models;

public record SetSiteAccessibilitiesRequest(string Site, IEnumerable<Accessibility> Accessibilities);
