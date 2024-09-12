using System.Collections.Generic;

namespace Nhs.Appointments.Api.Models;

public record UserProfile(string EmailAddress, IEnumerable<UserProfileSite> AvailableSites);

public record UserProfileSite(string Id, string Name, string Address);