using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Models;

public record CancelBookingRequest(
    string bookingReference, 
    string site,
    [property: JsonPropertyName("cancellationReason")]
    CancellationReason? cancellationReason,
    [property: JsonProperty("additionalData")]
    object additionalData = null
);
