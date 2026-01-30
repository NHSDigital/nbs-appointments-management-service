using Gherkin.Ast;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.BulkImport;
[FeatureFile("./Scenarios/BulkImport/SiteStatusBulkImport.feature")]
public class SiteSoftDeletionFeatureSteps : BaseBulkImportFeatureSteps
{
    private const string Headers = "Name,Id";

    [When("I bulk update the soft deletion status of the following sites")]
    public async Task ToggleSoftDeletionAsync(DataTable dataTable)
    {
        var csv = BuildInputCsv(dataTable, Headers);

        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(csv);

        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        content.Add(fileContent, "file", "user-upload.csv");

        Response = await GetHttpClientForTest().PostAsync("http://localhost:7071/api/site-deletion/import", content);
    }
}
