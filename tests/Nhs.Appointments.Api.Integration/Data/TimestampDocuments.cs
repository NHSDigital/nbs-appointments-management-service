using System;
using Newtonsoft.Json;
using Nhs.Appointments.Audit.Persistance;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Api.Integration.Data;

public class DailyAvailabilityTimestamped : DailyAvailabilityDocument
{
    [JsonProperty("_ts")]
    public long RawTimestamp { get; set; }

    public DateTimeOffset CosmosTimestamp => DateTimeOffset.FromUnixTimeSeconds(RawTimestamp);
}

public class BookingTimestamped : BookingDocument
{
    [JsonProperty("_ts")]
    public long RawTimestamp { get; set; }

    public DateTimeOffset CosmosTimestamp => DateTimeOffset.FromUnixTimeSeconds(RawTimestamp);
}

public class BookingIndexTimestamped : BookingIndexDocument
{
    [JsonProperty("_ts")]
    public long RawTimestamp { get; set; }

    public DateTimeOffset CosmosTimestamp => DateTimeOffset.FromUnixTimeSeconds(RawTimestamp);
}

public class UserTimestamped : UserDocument
{
    [JsonProperty("_ts")]
    public long RawTimestamp { get; set; }

    public DateTimeOffset CosmosTimestamp => DateTimeOffset.FromUnixTimeSeconds(RawTimestamp);
}

public class SiteTimestamped : SiteDocument
{
    [JsonProperty("_ts")]
    public long RawTimestamp { get; set; }

    public DateTimeOffset CosmosTimestamp => DateTimeOffset.FromUnixTimeSeconds(RawTimestamp);
}

public class AuditNotificationTimestamped : AuditNotificationDocument
{
    [JsonProperty("_ts")]
    public long RawTimestamp { get; set; }

    public DateTimeOffset CosmosTimestamp => DateTimeOffset.FromUnixTimeSeconds(RawTimestamp);
}

public class AuditFunctionDocumentTimestamped : AuditFunctionDocument
{
    [JsonProperty("_ts")]
    public long RawTimestamp { get; set; }

    public DateTimeOffset CosmosTimestamp => DateTimeOffset.FromUnixTimeSeconds(RawTimestamp);
}
