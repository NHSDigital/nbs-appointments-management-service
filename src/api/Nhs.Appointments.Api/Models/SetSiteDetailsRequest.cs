using System.Collections.Generic;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Models;

public record SetSiteDetailsRequest(string Site, string Name);
