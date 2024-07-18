using System.Collections.Generic;

namespace Nhs.Appointments.Api.Models;

public record GetRolesResponse(IEnumerable<GetRoleResponseItem> Roles);

public record GetRoleResponseItem(string DisplayName, string Id);