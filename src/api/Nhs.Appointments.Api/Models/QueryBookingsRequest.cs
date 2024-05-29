using System;

namespace Nhs.Appointments.Api.Models;

public record QueryBookingsRequest(DateTime from, DateTime to, string site);    
