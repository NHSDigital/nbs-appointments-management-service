using System;

namespace Nhs.Appointments.Api.Models;

public record MakeBookingResponse(string BookingReference, bool Provisional, Uri? ConfirmationEndpoint);
