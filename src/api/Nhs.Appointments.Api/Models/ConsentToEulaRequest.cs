using System;

namespace Nhs.Appointments.Api.Models;

public record ConsentToEulaRequest(string userId, DateOnly versionDate);

