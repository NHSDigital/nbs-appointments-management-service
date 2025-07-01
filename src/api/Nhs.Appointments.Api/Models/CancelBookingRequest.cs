using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Models;

public record CancelBookingRequest(string bookingReference, string site, CancellationReason cancellationReason);
