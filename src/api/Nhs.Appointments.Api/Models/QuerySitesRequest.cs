using Newtonsoft.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Models;
public record QuerySitesRequest(
    [property:JsonProperty("filters", Required = Required.Always)]
    SiteFilter[] Filters,
    int MaxRecords = 50,
    bool IgnoreCache = true);
