using System;
using Newtonsoft.Json;

namespace Nhs.Appointments.Api.Models;

public record SetTemplateAssignmentRequest
{
    [JsonProperty("site")]
    public string Site { get; set; }

    [JsonProperty("assignments")]
    public TemplateAssignment[] Assignments { get; set; }
}

public record TemplateAssignment(
        [JsonProperty("from")] string From,
        [JsonProperty("until")] string Until,
        [JsonProperty("templateId")] string TemplateId)
{
    public DateOnly FromDate => DateOnly.ParseExact(From, "yyyy-MM-dd");
    public DateOnly UntilDate => DateOnly.ParseExact(Until, "yyyy-MM-dd");
}