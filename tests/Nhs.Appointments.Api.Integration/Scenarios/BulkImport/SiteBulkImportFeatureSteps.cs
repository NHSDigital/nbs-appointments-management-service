using FluentAssertions;
using Gherkin.Ast;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core.Sites;
using Nhs.Appointments.Persistance.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;
using Location = Nhs.Appointments.Core.Sites.Location;

namespace Nhs.Appointments.Api.Integration.Scenarios.BulkImport;

[FeatureFile("./Scenarios/BulkImport/SiteBulkImport.feature")]
public class SiteBulkImportFeatureSteps : BaseBulkImportFeatureSteps, IDisposable
{
    private Site ActualResponse { get; set; }
    private static string[] TestSiteIds { get; set; }

    [When("I import the following sites")]
    public async Task ImportSite(DataTable dataTable)
    {
        const string sitesHeader =
            "OdsCode,Name,Address,PhoneNumber,Longitude,Latitude,ICB,Region,Site type,accessible_toilet,braille_translation_service,disabled_car_parking,car_parking,induction_loop,sign_language_service,step_free_access,text_relay,wheelchair_access,Id";

        var csv = BuildSiteInputCsv(dataTable, sitesHeader);

        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(csv);

        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        content.Add(fileContent, "file", "user-upload.csv");

        Response = await Http.PostAsync("http://localhost:7071/api/site/import", content);
        TestSiteIds = [.. dataTable.Rows.Skip(1).Select(r => r.Cells.ElementAt(18).Value)];
    }

    [When("I request site information for site '(.+)'")]
    public async Task RequestSites(string siteId)
    {
        Response = await Http.GetAsync($"http://localhost:7071/api/sites/{siteId}");
    }

    [Then("the correct site is returned")]
    public async Task AssertSite(DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var expectedSite = new Site(
            Id: row.Cells.ElementAt(0).Value,
            Name: row.Cells.ElementAt(1).Value,
            Address: row.Cells.ElementAt(2).Value,
            PhoneNumber: row.Cells.ElementAt(3).Value,
            OdsCode: row.Cells.ElementAt(4).Value,
            Region: row.Cells.ElementAt(5).Value,
            IntegratedCareBoard: row.Cells.ElementAt(6).Value,
            InformationForCitizens: row.Cells.ElementAt(7).Value,
            Accessibilities: ParseAccessibilities(row.Cells.ElementAt(8).Value),
            location: new Location("Point", [double.Parse(row.Cells.ElementAt(9).Value), double.Parse(row.Cells.ElementAt(10).Value)]),
            status: null,
            isDeleted: dataTable.GetBoolRowValueOrDefault(row, "IsDeleted"),
            Type: dataTable.GetRowValueOrDefault(row, "Type")
        );
        Response.StatusCode.Should().Be(HttpStatusCode.OK);
        (_, ActualResponse) =
            await JsonRequestReader.ReadRequestAsync<Site>(await Response.Content.ReadAsStreamAsync());
        ActualResponse.Should().BeEquivalentTo(expectedSite, opts => opts.Excluding(x => x.isDeleted));
    }

    public Task InitializeAsync() => throw new System.NotImplementedException();

    public void Dispose()
    {
        foreach (var siteId in TestSiteIds)
        {
            DeleteSiteData(Client, siteId).GetAwaiter().GetResult();
        }
    }

    private byte[] BuildSiteInputCsv(DataTable dataTable, string headers)
    {
        var sb = new StringBuilder(headers);

        sb.Append("\r\n");

        foreach (var row in dataTable.Rows.Skip(1))
        {
            var cells = row.Cells.Select(c => EscapeCsv(c.Value)).ToList();

            sb.Append($"{string.Join(",", cells)}\r\n");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static async Task DeleteSiteData(CosmosClient cosmosClient, string testId)
    {
        const string partitionKey = "site";
        var container = cosmosClient.GetContainer("appts", "core_data");
        using var feed = container.GetItemLinqQueryable<SiteDocument>().Where(sd => sd.Id.Contains(testId))
            .ToFeedIterator();
        while (feed.HasMoreResults)
        {
            var documentsResponse = await feed.ReadNextAsync();
            foreach (var document in documentsResponse)
            {
                await container.DeleteItemStreamAsync(document.Id, new PartitionKey(partitionKey));
            }
        }
    }
}
