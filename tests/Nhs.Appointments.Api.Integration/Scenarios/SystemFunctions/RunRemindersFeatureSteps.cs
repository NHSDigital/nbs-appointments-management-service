using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.SystemFunctions;

[FeatureFile("./Scenarios/SystemFunctions/RunReminders.feature")]
public class RunRemindersFeatureSteps : BaseFeatureSteps
{
    [When("the reminders job runs")]
    public async Task RunRemindersJob()
    {
        var response = await Http.PostAsync("http://localhost:7071/api/system/run-reminders", new StringContent(""));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Then("there are no errors")]
    public Task AssertOk()
    {
        return Task.CompletedTask;
    }

    [And("those appointments have already had notifications sent")]
    public async Task SetReminderSentFlag()
    {
        var container = Client.GetContainer("appts", "booking_data");
        foreach (var item in MadeBookings)
        {
            var patch = PatchOperation.Set("/reminderSent", true);
            var result = await container.PatchItemAsync<BookingDocument>(
                id: item.Reference,
                partitionKey: new PartitionKey(item.Site),
                patchOperations: [patch]);
        }
    }

    [And("there are audit entries in the database")]
    public async Task AddAuditEntriesToTheDatabase()
    {
        var auditEntry = new AvailabilityCreatedEventDocument
        {
            Id = Guid.NewGuid().ToString(),
            DocumentType = "availiblity_created",
            From = ParseNaturalLanguageDateOnly("2 days from today"),
            By = "someone",
            Site = GetSiteId(),
            Created = DateTime.UtcNow
        };
        await Client.GetContainer("appts", "booking_data").CreateItemAsync(auditEntry);
    }

    [Then("the following notifications are sent out")]
    public async Task AssertNotifications(DataTable dataTable)
    {
        var data = dataTable.Rows.Skip(1).Select(r => (
            r.Cells.ElementAt(0).Value, 
            r.Cells.ElementAt(1).Value, 
            r.Cells.ElementAt(2).Value, 
            r.Cells.ElementAt(3).Value, 
            r.Cells.ElementAt(4).Value
            )
        );
        foreach (var item in data)
        {
            var contactItemType = Enum.Parse<ContactItemType>(item.Item1, true);

            var contactDetails = GetContactInfo(contactItemType);
            var notifications = await GetNotificationsForRecipient(contactDetails);
            
            var customId = CreateCustomBookingReference(item.Item5);

            var notification = notifications.Single();
            notification.templateId.Should().Be(item.Item2);
            notification.templateValues["firstName"].Should().Be("FirstName");
            notification.templateValues["vaccine"].Should().Be(item.Item3);
            notification.templateValues["serviceURL"].Should().Be(item.Item4);
            notification.templateValues["reference"].Should().Be(customId);
        }
    }

    [Then("no notifications are sent out")]
    public async Task AssertNotificationsAreNotSent()
    {
        var allNotifications = new List<NotificationData>();
        allNotifications.AddRange(await GetNotificationsForRecipient(GetContactInfo(ContactItemType.Phone)));
        allNotifications.AddRange(await GetNotificationsForRecipient(GetContactInfo(ContactItemType.Email)));
        allNotifications.AddRange(await GetNotificationsForRecipient(GetContactInfo(ContactItemType.Landline)));

        allNotifications.Count().Should().Be(0);
    }

    [And("I have Clinical Services")]
    public async Task SetUpClinicalServices(DataTable dataTable)
    {
        var clinicalServices = dataTable.Rows.Skip(1).Select(x => new ClinicalServiceTypeDocument()
        {
            Id = x.Cells.ElementAt(0).Value,
            Label = x.Cells.ElementAt(0).Value,
            ServiceType = x.Cells.ElementAt(1).Value,
            Url = x.Cells.ElementAt(2).Value
        });

        var clinicalServicesDocument = new ClinicalServiceDocument()
        {
            Id = "clinical_services",
            DocumentType = "system",
            Services = clinicalServices.ToArray()
        };

        await Client.GetContainer("appts", "core_data").UpsertItemAsync(clinicalServicesDocument);
    }

    private Task<IEnumerable<NotificationData>> GetNotificationsForRecipient(string contactInfo)
    {
        var container = Client.GetContainer("appts", "local_notifications");
        return RunQueryAsync<NotificationData>(container, nd => nd.recipient == contactInfo);
    }

    private string ResolveEventName(string shortEventName) => shortEventName switch
    {
        "Reminder" => "BookingReminder",
        _ => throw new ArgumentOutOfRangeException()
    };

    private string ResolveServiceName(string shortServiceName) => shortServiceName switch
    {
        "COVID" => "COVID:18_74",
        _ => throw new ArgumentOutOfRangeException()
    };
}

public record NotificationData(string recipient, string templateId, Dictionary<string, object> templateValues);
