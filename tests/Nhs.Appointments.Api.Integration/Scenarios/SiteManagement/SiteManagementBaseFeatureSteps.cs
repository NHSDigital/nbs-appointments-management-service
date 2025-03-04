using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;
using Location = Nhs.Appointments.Core.Location;

namespace Nhs.Appointments.Api.Integration.Scenarios.SiteManagement;

public abstract class SiteManagementBaseFeatureSteps : BaseFeatureSteps
{
    private ErrorMessageResponseItem ErrorResponse { get; set; }

    private IEnumerable<ErrorMessageResponseItem> ErrorResponses { get; set; }
    protected HttpResponseMessage Response { get; set; }
    protected Site ActualResponse { get; set; }

    [Given("The site '(.+)' does not exist in the system")]
    public Task NoSite()
    {
        return Task.CompletedTask;
    }

    [Given("The following sites exist in the system")]
    public async Task SetUpSites(DataTable dataTable)
    {
        var sites = dataTable.Rows.Skip(1).Select(
            row => new SiteDocument
            {
                Id = GetSiteId(row.Cells.ElementAt(0).Value),
                Name = row.Cells.ElementAt(1).Value,
                Address = row.Cells.ElementAt(2).Value,
                PhoneNumber = row.Cells.ElementAt(3).Value,
                OdsCode = row.Cells.ElementAt(4).Value,
                Region = row.Cells.ElementAt(5).Value,
                IntegratedCareBoard = row.Cells.ElementAt(6).Value,
                InformationForCitizens = row.Cells.ElementAt(7).Value,
                DocumentType = "site",
                Accessibilities = ParseAccessibilities(row.Cells.ElementAt(8).Value),
                Location = new Location("Point",
                    new[] { double.Parse(row.Cells.ElementAt(9).Value), double.Parse(row.Cells.ElementAt(10).Value) }),
            });

        foreach (var site in sites)
        {
            await Client.GetContainer("appts", "core_data").UpsertItemAsync(site);
        }
    }

    [Then("a message is returned saying the site is not found")]
    public async Task AssertNotFound()
    {
        Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (_, ErrorResponse) =
            await JsonRequestReader.ReadRequestAsync<ErrorMessageResponseItem>(
                await Response.Content.ReadAsStreamAsync());
        ErrorResponse.Message.Should().Be("The specified site was not found.");
    }

    [Then("a bad request response is returned with the following error messages")]
    public async Task AssertBadRequest(DataTable dataTable)
    {
        Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var row = dataTable.Rows.ElementAt(0);

        var expectedErrorMessages = row.Cells.Select(x=>x.Value).ToList();

        (_, ErrorResponses) =
            await JsonRequestReader.ReadRequestAsync<IEnumerable<ErrorMessageResponseItem>>(
                await Response.Content.ReadAsStreamAsync());
        var actualErrorMessages = ErrorResponses.Select(x => x.Message).ToList();
        actualErrorMessages.Should().BeEquivalentTo(expectedErrorMessages);
    }

    [Then("the correct information for site '(.+)' is returned")]
    public async Task Assert(DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var siteId = row.Cells.ElementAt(0).Value;
        var expectedSite = new Site(
            Id: GetSiteId(siteId),
            Name: row.Cells.ElementAt(1).Value,
            Address: row.Cells.ElementAt(2).Value,
            PhoneNumber: row.Cells.ElementAt(3).Value,
            OdsCode: row.Cells.ElementAt(4).Value,
            Region: row.Cells.ElementAt(5).Value,
            IntegratedCareBoard: row.Cells.ElementAt(6).Value,
            InformationForCitizens: row.Cells.ElementAt(7).Value,
            Accessibilities: ParseAccessibilities(row.Cells.ElementAt(8).Value),
            Location: new Location(
                Type: "Point",
                Coordinates: [double.Parse(row.Cells.ElementAt(9).Value), double.Parse(row.Cells.ElementAt(10).Value)])
        );
        Response.StatusCode.Should().Be(HttpStatusCode.OK);

        var actualResult = await Client.GetContainer("appts", "core_data")
            .ReadItemAsync<Site>(GetSiteId(siteId), new PartitionKey("site")); 
        actualResult.Resource.Should().BeEquivalentTo(expectedSite, opts => opts.WithStrictOrdering());
    }

    protected static Accessibility[] ParseAccessibilities(string accessibilities)
    {
        if (accessibilities == "__empty__")
        {
            return Array.Empty<Accessibility>();
        }

        var pairs = accessibilities.Split(",");
        return pairs.Select(p => p.Trim().Split("=")).Select(kvp => new Accessibility(kvp[0], kvp[1])).ToArray();
    }
}
