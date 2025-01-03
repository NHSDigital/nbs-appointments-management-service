﻿namespace Nhs.Appointments.Core;

public interface IAvailabilityStore
{
    Task<IEnumerable<SessionInstance>> GetSessions(string site, DateOnly notBefore, DateOnly notAfter);
    Task ApplyAvailabilityTemplate(string site, DateOnly date, Session[] sessions, ApplyAvailabilityMode mode);
    Task<IEnumerable<DailyAvailability>> GetDailyAvailability(string site, DateOnly from, DateOnly to);
}