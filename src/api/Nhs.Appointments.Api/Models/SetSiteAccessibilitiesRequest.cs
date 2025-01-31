using System.Collections.Generic;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Models;

public record SetSiteAccessibilitiesRequest(string Site, IEnumerable<Accessibility> Accessibilities);
