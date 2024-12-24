using DotMarkdown;

namespace CsvDataTool;

public class SiteReportWriter(FileInfo output)
{
    public void Write(IEnumerable<SiteRowReportItem> report, int errorRowLimit = 0)
    {
        var totalRowCount = report.GroupBy(r => r.Index).Count();
        
        using (var reportWriter = MarkdownWriter.Create(output.OpenWrite()))
        {
            reportWriter.WriteHeading1("Csv Converter Report");
            reportWriter.WriteBold(totalRowCount.ToString());
            reportWriter.WriteString($" rows in import data");
            reportWriter.WriteLine();
            reportWriter.WriteLine();
            reportWriter.WriteBold(report.Count(r => r.Success).ToString());
            reportWriter.WriteString($" items converted successfully");
            reportWriter.WriteLine();
            reportWriter.WriteLine();

            if (report.Any(r => r.Success == false))
            {
                reportWriter.WriteHeading2("Conversion errors");
                var errors = report.Where(r => r.Success == false).GroupBy(r => r.Index);
                if(errorRowLimit > 0) 
                    errors = errors.Take(errorRowLimit);

                foreach (var errorGroup in errors)
                {
                    
                    reportWriter.WriteHeading3(GetItemName(errorGroup.ElementAt(0)));
                    foreach (var error in errorGroup)
                    {
                        reportWriter.WriteBulletItem(error.Message);
                    }
                    
                    reportWriter.WriteLine();
                    reportWriter.WriteLine();
                }
            }
        }
    }
    
    private string GetItemName(SiteRowReportItem item) => String.IsNullOrEmpty(item.Name) ? $"Row {item.Index+1} (no name)" : item.Name;
}
