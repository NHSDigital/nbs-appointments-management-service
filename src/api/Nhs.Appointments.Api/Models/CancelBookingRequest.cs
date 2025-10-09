using System.Text.Json.Serialization;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Models;

public record CancelBookingRequest(
    string bookingReference, 
    string site,
    CancellationReason? cancellationReason,
    object additionalData = null
) : CancelBookingOpenApiRequest(cancellationReason, additionalData);

public record CancelBookingOpenApiRequest(
    [property: JsonPropertyName("cancellationReason")]
    CancellationReason? cancellationReason,
    [property: JsonPropertyName("additionalData")]
    object additionalData);
