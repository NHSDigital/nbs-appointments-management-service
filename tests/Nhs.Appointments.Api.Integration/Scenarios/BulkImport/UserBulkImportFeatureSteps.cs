using Gherkin.Ast;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.BulkImport;

[FeatureFile("./Scenarios/BulkImport/UserBulkImport.feature")]
public class UserBulkImportFeatureSteps : BaseBulkImportFeatureSteps
{
    [When("I import the following users")]
    public async Task ImportUsers(DataTable dataTable)
    {
        const string usersHeader = "User,FirstName,LastName,appointment-manager,availability-manager,site-details-manager,user-manager,Region,Site,ICB";

        var csv = BuildInputCsv(dataTable, usersHeader);

        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(csv);

        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        content.Add(fileContent, "file", "user-upload.csv");

        Response = await Http.PostAsync("http://localhost:7071/api/user/import", content);
    }
}
