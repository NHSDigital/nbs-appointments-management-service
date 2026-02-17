using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Nhs.Appointments.Api.Integration.Data;
using Nhs.Appointments.Persistance.Models;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios;

public abstract class AggregateFeatureSteps : BaseFeatureSteps
{
    [Then(@"an aggregation updated recently for site '(.*)', date '(.*)', '(.*)' cancelled bookings, and maximumCapacity '(.*)', and with service details")]
    [And(@"an aggregation updated recently for site '(.*)', date '(.*)', '(.*)' cancelled bookings, and maximumCapacity '(.*)', and with service details")]
    public async Task AssertAggregationDocumentUpdatedRecentlyFor(string site, string day, int expectedCancelled, int expectedMaximumCapacity, DataTable servicesTable)
    {
        var expectedServiceSummaries = servicesTable.Rows.Skip(1).Select((row, index) =>
        {
            var service = servicesTable.GetRowValueOrDefault(row, "Service");
            
            var bookings = servicesTable.GetIntRowValueOrDefault(row, "Bookings", 0);
            var orphaned = servicesTable.GetIntRowValueOrDefault(row, "Orphaned", 0);
            var remainingCapacity = servicesTable.GetIntRowValueOrDefault(row, "RemainingCapacity", 0);

            return new DailySiteServiceSummary(service, bookings, orphaned, remainingCapacity);
        }).ToList();

        var actualAggregationDocument = await FetchRecentAggregationDocument(GetSiteId(site), NaturalLanguageDate.Parse(day));

        var expectedAggregationDocument = new DailySiteSummaryDocument
        {
            DocumentType = "daily-site-summary-report",
            Id = actualAggregationDocument.Id,
            Date = NaturalLanguageDate.Parse(day),
            Cancelled =  expectedCancelled,
            MaximumCapacity = expectedMaximumCapacity,
            Bookings = expectedServiceSummaries.ToDictionary(x => x.Service, x => x.Booked),
            RemainingCapacity = expectedServiceSummaries.ToDictionary(x => x.Service, x => x.RemainingCapacity),
            //orphaned is empty if no services are orphaned...?
            Orphaned = expectedServiceSummaries.Where(x => x.Orphaned > 0).ToDictionary(x => x.Service, x => x.Orphaned)
        };
        
        actualAggregationDocument.Should().BeEquivalentTo(expectedAggregationDocument, options => options.Excluding(x => x.GeneratedAtUtc));
    }
    
    [Then(@"an aggregation is created for site '(.*)', date '(.*)', '(.*)' cancelled bookings, and maximumCapacity '(.*)', and with service details")]
    [And(@"an aggregation is created for site '(.*)', date '(.*)', '(.*)' cancelled bookings, and maximumCapacity '(.*)', and with service details")]
    public async Task AssertAggregationDocumentCreatedFor(string site, string day, int expectedCancelled, int expectedMaximumCapacity, DataTable servicesTable)
    {
        var expectedServiceSummaries = servicesTable.Rows.Skip(1).Select((row, index) =>
        {
            var service = servicesTable.GetRowValueOrDefault(row, "Service");
            
            var bookings = servicesTable.GetIntRowValueOrDefault(row, "Bookings", 0);
            var orphaned = servicesTable.GetIntRowValueOrDefault(row, "Orphaned", 0);
            var remainingCapacity = servicesTable.GetIntRowValueOrDefault(row, "RemainingCapacity", 0);

            return new DailySiteServiceSummary(service, bookings, orphaned, remainingCapacity);
        }).ToList();

        var actualAggregationDocument = await FetchRecentAggregationDocument(GetSiteId(site), NaturalLanguageDate.Parse(day));

        var expectedAggregationDocument = new DailySiteSummaryDocument
        {
            DocumentType = "daily-site-summary-report",
            Id = actualAggregationDocument.Id,
            Date = NaturalLanguageDate.Parse(day),
            Cancelled =  expectedCancelled,
            MaximumCapacity = expectedMaximumCapacity,
            Bookings = expectedServiceSummaries.ToDictionary(x => x.Service, x => x.Booked),
            RemainingCapacity = expectedServiceSummaries.ToDictionary(x => x.Service, x => x.RemainingCapacity),
            //orphaned is empty if no services are orphaned...?
            Orphaned = expectedServiceSummaries.Where(x => x.Orphaned > 0).ToDictionary(x => x.Service, x => x.Orphaned)
        };
        
        actualAggregationDocument.Should().BeEquivalentTo(expectedAggregationDocument, options => options.Excluding(x => x.GeneratedAtUtc));
    }
    
    [Then(@"an aggregation did not update recently for site '(.*)' on date '(.*)'")]
    [And(@"an aggregation did not update recently for site '(.*)' on date '(.*)'")]
    public async Task AssertNoAggregationDocumentFor(string site, string day)
    {
        await Assert.ThrowsAsync<TimeoutException>(async () => await FetchRecentAggregationDocument(GetSiteId(site), NaturalLanguageDate.Parse(day))); 
    }

    private async Task<DailySiteSummaryDocument> FetchRecentAggregationDocument(string id, DateOnly date)
    {
        var dailySiteSummaryDocument = await FindItemWithRetryAsync(id, date, _actionTimestamp, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(1));

        //confirm that the document was created recently, as cannot easily verify via id query
        dailySiteSummaryDocument.GeneratedAtUtc.Should().BeBefore(_actionTimestamp.AddSeconds(3));
        dailySiteSummaryDocument.GeneratedAtUtc.Should().BeOnOrAfter(_actionTimestamp);

        return dailySiteSummaryDocument;
    }

    private async Task<DailySiteSummaryDocument> FindItemWithRetryAsync(string id, DateOnly date, DateTimeOffset generatedAfter, TimeSpan timeout, TimeSpan delay)
    {
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < timeout)
        {
            var documentsFound = await CosmosQueryFeed<DailySiteSummaryDocument>("aggregated_data",
                d => d.Id == id && d.Date == date && d.GeneratedAtUtc >= generatedAfter);

            if (documentsFound.Count() > 0)
            {
                return documentsFound.OrderByDescending(doc => doc.GeneratedAtUtc).First();
            }

            await Task.Delay(delay); // Wait before retrying
        }

        throw new TimeoutException("DailySiteSummaryDocument not found within the timeout period.");
    }
    
    private record DailySiteServiceSummary(string Service, int Booked, int Orphaned, int RemainingCapacity);
}


