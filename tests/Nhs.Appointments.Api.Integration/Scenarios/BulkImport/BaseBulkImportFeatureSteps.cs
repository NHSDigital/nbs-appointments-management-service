using FluentAssertions;
using Gherkin.Ast;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nhs.Appointments.Core;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.BulkImport;
public abstract class BaseBulkImportFeatureSteps : BaseFeatureSteps
{
    protected HttpResponseMessage Response { get; set; }

    [Then("I receive a report that the import was successful")]
    public async Task AssertSuccessfulBulkImport()
    {
        Response.EnsureSuccessStatusCode();

        var result = JsonConvert.DeserializeObject<IEnumerable<ReportItem>>(await Response.Content.ReadAsStringAsync());

        result.All(r => r.Success).Should().BeTrue();
    }

    [Then(@"the call should fail with (\d*)")]
    public void AssertFailureCode(int statusCode) => Response.StatusCode.Should().Be((HttpStatusCode)statusCode);

    protected byte[] BuildInputCsv(DataTable dataTable, string headers)
    {
        var sb = new StringBuilder(headers);

        sb.Append("\r\n");

        foreach (var row in dataTable.Rows.Skip(1))
        {
            var cells = row.Cells.Select(c => EscapeCsv(c.Value)).ToList();
            cells.Add(EscapeCsv(_testId.ToString()));

            sb.Append($"{string.Join(",", cells)}\r\n");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            value = value.Replace("\"", "\"\"");
            return $"\"{value}\"";
        }
        return value;
    }
}
