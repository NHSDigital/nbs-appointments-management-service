﻿using Nhs.Appointments.Persistance.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Persistance;

public class AvailabilityDocumentStore(ITypedDocumentCosmosStore<DailyAvailabilityDocument> documentStore) : IAvailabilityDocumentStore
{
    public async Task<IEnumerable<SessionInstance>> GetSessions(string site, DateOnly from, DateOnly to)
    {
        var results = new List<SessionInstance>();
        var documents = await documentStore.RunQueryAsync<DailyAvailabilityDocument>(b => b.Site == site);
        foreach (var day in documents)
        {
            results.AddRange(day.Sessions.Select(
                s => new SessionInstance(day.Date.ToDateTime(s.From), day.Date.ToDateTime(s.Until))
                {
                    Services = s.Services,
                    SessionHolder = "default",
                    SlotLength = s.SlotLength,
                    Capacity = s.Capacity
                }
                ));
        }
        return results;
    }
}