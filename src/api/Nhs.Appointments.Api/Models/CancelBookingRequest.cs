using Nhs.Appointments.Core;
using System.Text.Json.Serialization;

namespace Nhs.Appointments.Api.Models;

public record CancelBookingRequest(
    string bookingReference, 
    string site,
    [property: JsonPropertyName("cancellationReason")]
    CancellationReason? cancellationReason
);
