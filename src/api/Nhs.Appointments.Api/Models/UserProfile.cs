using Nhs.Appointments.Core;
using System.Collections.Generic;

namespace Nhs.Appointments.Api.Models;

public record UserProfile(string EmailAddress, IEnumerable<Site> AvailableSites);