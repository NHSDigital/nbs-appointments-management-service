using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Functions.Worker;
using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Api.Functions.ChangeFeedFunctions;

public class AvailabilityRecurrenceFunction(IAvailabilityWriteService availabilityWriteService)
{
    [Function("AvailabilityRecurrence")]
    [AllowAnonymous]
    public async Task AvailabilityRecurrence([CosmosDBTrigger(
            databaseName: "appts",
            containerName: "availability_recurrence_data",
            LeaseContainerName = "availability_recurrence_data_lease",
            Connection = "changefeed", 
            CreateLeaseContainerIfNotExists = true
        )] IReadOnlyList<AvailabilityCreatedEventDocument> createdEvents,
        FunctionContext context)
    {
        if (createdEvents is not null && createdEvents.Any())
        {
            foreach (var createdEvent in createdEvents)
            {
                await ApplyTemplate(createdEvent);
            }
        }
    }

    private async Task ApplyTemplate(AvailabilityCreatedEventDocument document)
    {
        var dates = GetDatesBetween(document.From, document.To ?? document.From, document.Template.Days);
        foreach (var date in dates)
        {
            await availabilityWriteService.SetAvailabilityAsync(date, document.Site, document.Sessions, ApplyAvailabilityMode.Additive);
        }
    }
    
    private static IEnumerable<DateOnly> GetDatesBetween(DateOnly start, DateOnly end, params DayOfWeek[] weekdays)
    {
        var cursor = start;
        while (cursor <= end)
        {
            if (weekdays == null || weekdays.Contains(cursor.DayOfWeek))
                yield return cursor;
            cursor = cursor.AddDays(1);
        }
    }
}
