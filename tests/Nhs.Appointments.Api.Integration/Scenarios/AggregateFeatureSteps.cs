using System;
using System.Linq;
using System.Linq.Expressions;
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
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _pollingTimeout = TimeSpan.FromSeconds(5);

    [Then(
        @"an aggregation updated recently for site '(.*)', date '(.*)', '(.*)' cancelled bookings, and maximumCapacity '(.*)', and with service details")]
    [And(
        @"an aggregation updated recently for site '(.*)', date '(.*)', '(.*)' cancelled bookings, and maximumCapacity '(.*)', and with service details")]
    public async Task AssertAggregationDocumentUpdatedRecentlyFor(string site, string day, int expectedCancelled,
        int expectedMaximumCapacity, DataTable servicesTable)
    {
        var expectedAggregationDocument =
            BuildExpectedDocument(day, expectedCancelled, expectedMaximumCapacity, servicesTable);
        var actualAggregationDocument = await FetchRecentAggregationDocumentWithDetails(GetSiteId(site),
            NaturalLanguageDate.Parse(day), expectedAggregationDocument);
        actualAggregationDocument.Should().NotBeNull();
    }

    [Then(
        @"an aggregation is created for site '(.*)', date '(.*)', '(.*)' cancelled bookings, and maximumCapacity '(.*)', and with service details")]
    [And(
        @"an aggregation is created for site '(.*)', date '(.*)', '(.*)' cancelled bookings, and maximumCapacity '(.*)', and with service details")]
    public async Task AssertAggregationDocumentCreatedFor(string site, string day, int expectedCancelled,
        int expectedMaximumCapacity, DataTable servicesTable)
    {
        var expectedAggregationDocument =
            BuildExpectedDocument(day, expectedCancelled, expectedMaximumCapacity, servicesTable);

        var actualAggregationDocument = await FetchAggregationDocumentWithDetails(GetSiteId(site),
            NaturalLanguageDate.Parse(day), expectedAggregationDocument);

        actualAggregationDocument.Should().NotBeNull();
    }

    [Then(
        @"an aggregation did not update recently for site '(.*)', date '(.*)', '(.*)' cancelled bookings, and maximumCapacity '(.*)', and with service details")]
    [And(
        @"an aggregation did not update recently for site '(.*)', date '(.*)', '(.*)' cancelled bookings, and maximumCapacity '(.*)', and with service details")]
    public async Task AssertNoRecentAggregationDocumentWith(string site, string day, int expectedCancelled,
        int expectedMaximumCapacity, DataTable servicesTable)
    {
        var expectedAggregationDocument =
            BuildExpectedDocument(day, expectedCancelled, expectedMaximumCapacity, servicesTable);
        await Assert.ThrowsAsync<TimeoutException>(async () =>
            await FetchRecentAggregationDocumentWithDetails(GetSiteId(site), NaturalLanguageDate.Parse(day),
                expectedAggregationDocument));
    }

    private async Task<DailySiteSummaryDocument> FetchRecentAggregationDocumentWithDetails(string id, DateOnly date,
        DailySiteSummaryDocument expectedAggregationDocument)
    {
        Expression<Func<DailySiteSummaryDocument, bool>> predicate = d =>
            d.Id == id && d.Date == date && d.GeneratedAtUtc >= _actionTimestamp;

        var dailySiteSummaryDocument = await FindItemWithDetailsRetryAsync(predicate, expectedAggregationDocument);

        //confirm that the document was created recently, as cannot easily verify via id query
        dailySiteSummaryDocument.GeneratedAtUtc.Should().BeBefore(_actionTimestamp.Add(_pollingTimeout));
        dailySiteSummaryDocument.GeneratedAtUtc.Should().BeOnOrAfter(_actionTimestamp);

        return dailySiteSummaryDocument;
    }

    private async Task<DailySiteSummaryDocument> FetchAggregationDocumentWithDetails(string id, DateOnly date,
        DailySiteSummaryDocument expectedAggregationDocument)
    {
        Expression<Func<DailySiteSummaryDocument, bool>> predicate = d => d.Id == id && d.Date == date;
        var dailySiteSummaryDocument = await FindItemWithDetailsRetryAsync(predicate, expectedAggregationDocument);
        return dailySiteSummaryDocument;
    }

    private async Task<DailySiteSummaryDocument> FindItemWithDetailsRetryAsync(
        Expression<Func<DailySiteSummaryDocument, bool>> predicate, DailySiteSummaryDocument expectedDetails)
    {
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < _pollingTimeout)
        {
            var documentsFound = (await CosmosQueryFeed("aggregated_data", predicate)).ToList();

            if (documentsFound.Count > 0)
            {
                var latestDocument = documentsFound.OrderByDescending(doc => doc.GeneratedAtUtc).First();

                if (DailySiteSummaryDocumentIsEquivalent(expectedDetails, latestDocument))
                {
                    return latestDocument;
                }
            }

            await Task.Delay(_pollingInterval); // Wait before retrying
        }

        throw new TimeoutException("DailySiteSummaryDocument not found within the timeout period.");
    }

    private bool DailySiteSummaryDocumentIsEquivalent(DailySiteSummaryDocument expected,
        DailySiteSummaryDocument actual)
    {
        return expected.Bookings.SequenceEqual(actual.Bookings)
               && expected.Orphaned.SequenceEqual(actual.Orphaned)
               && expected.RemainingCapacity.SequenceEqual(actual.RemainingCapacity)
               && expected.Cancelled == actual.Cancelled
               && expected.MaximumCapacity == actual.MaximumCapacity;
    }

    private DailySiteSummaryDocument BuildExpectedDocument(string day, int expectedCancelled,
        int expectedMaximumCapacity, DataTable servicesTable)
    {
        var expectedServiceSummaries = servicesTable.Rows.Skip(1).Select((row, index) =>
        {
            var service = servicesTable.GetRowValueOrDefault(row, "Service");

            var bookings = servicesTable.GetIntRowValueOrDefault(row, "Bookings", 0);
            var orphaned = servicesTable.GetIntRowValueOrDefault(row, "Orphaned", 0);
            var remainingCapacity = servicesTable.GetIntRowValueOrDefault(row, "RemainingCapacity", 0);

            return new DailySiteServiceSummary(service, bookings, orphaned, remainingCapacity);
        }).ToList();

        var expectedAggregationDocument = new DailySiteSummaryDocument
        {
            DocumentType = "daily-site-summary-report",
            Date = NaturalLanguageDate.Parse(day),
            Cancelled = expectedCancelled,
            MaximumCapacity = expectedMaximumCapacity,
            Bookings = expectedServiceSummaries.ToDictionary(x => x.Service, x => x.Booked),
            RemainingCapacity = expectedServiceSummaries.ToDictionary(x => x.Service, x => x.RemainingCapacity),
            //orphaned is empty if no services are orphaned...?
            Orphaned = expectedServiceSummaries.Where(x => x.Orphaned > 0).ToDictionary(x => x.Service, x => x.Orphaned)
        };

        return expectedAggregationDocument;
    }

    private record DailySiteServiceSummary(string Service, int Booked, int Orphaned, int RemainingCapacity);
}
