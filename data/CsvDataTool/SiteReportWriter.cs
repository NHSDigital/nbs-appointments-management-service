using System.Text;

namespace CsvDataTool;

public class SiteReportWriter(FileInfo output)
{
    public async Task Write(IEnumerable<SiteRowReportItem> report)
    {
        var result = new StringBuilder("<html><body><table>");
        result.AppendLine("<tr style=\"background-color:#cfcfcf\"><th>Index</th><th>Name</th><th>Success</th><th>Information</th></tr>");
        foreach(var item in report)
        {
            var bgcolour = item.Success ? "#aff0da" : "#f76363";
            result.AppendLine($"<tr style=\"background-color:{bgcolour}\"><td>{item.Index}</td><td>{item.Name}</td><td>{(item.Success ? "Yes" : "No")}</td><td>{item.Message}</td></tr>");
        }

        result.AppendLine("</table></body></html>");

        using (var writer = output.OpenWrite())
        {
            await writer.WriteAsync(Encoding.UTF8.GetBytes(result.ToString()));
        }
    }
}

