using System;

namespace Nhs.Appointments.Api.Models;

public record UserProfile(string EmailAddress, DateOnly? LatestAcceptedEulaVersion);

