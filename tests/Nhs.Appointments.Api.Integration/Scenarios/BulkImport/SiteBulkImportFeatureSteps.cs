using Gherkin.Ast;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.BulkImport;

[FeatureFile("./Scenarios/BulkImport/SiteBulkImport.feature")]
public class SiteBulkImportFeatureSteps : BaseBulkImportFeatureSteps
{
    [When("I import the following site")]
    public async Task ImportSite(DataTable dataTable)
    {
        const string sitesHeader =
            "OdsCode,Name,Address,PhoneNumber,Longitude,Latitude,ICB,Region,Site type,accessible_toilet,braille_translation_service,disabled_car_parking,car_parking,induction_loop,sign_language_service,step_free_access,text_relay,wheelchair_access,Id";

        var csv = BuildInputCsv(dataTable, sitesHeader);

        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(csv);

        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        content.Add(fileContent, "file", "user-upload.csv");

        Response = await Http.PostAsync("http://localhost:7071/api/site/import", content);
    }
}
