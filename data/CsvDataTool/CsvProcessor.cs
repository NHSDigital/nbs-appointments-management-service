using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace CsvDataTool;

public class CsvProcessor<TDocument, TMap>(
    Func<TDocument, Task> processRow,
    Func<TDocument, string> getItemName,
    Func<TDocument, TDocument> mutateDocument = null)
    where TMap : ClassMap
{
    public async Task<IEnumerable<ReportItem>> ProcessFile(TextReader csvReader)
    {
        var index = -1;
        var report = new List<ReportItem>();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            ShouldSkipRecord = args =>
            {
                index++;
                return false;
            },
            ReadingExceptionOccurred = args =>
            {
                report.Add(new ReportItem(index, "", false, args.Exception.ToString()));

                return false;
            },
            BadDataFound = args =>
            {
                report.Add(new ReportItem(index, "", false, $"Bad data found in field '{args.Field}'"));
            },
            Delimiter = ",",
            Quote = '"'
        };

        using (var csv = new CsvReader(csvReader, config))
        {
            csv.Context.RegisterClassMap<TMap>();

            var imported = mutateDocument != null
                ? csv.GetRecords<TDocument>().Select(mutateDocument)
                : csv.GetRecords<TDocument>();

            foreach (var item in imported)
            {
                try
                {
                    await processRow(item);
                    report.Add(new ReportItem(index, getItemName(item), true, ""));
                }
                catch (Exception ex)
                {
                    report.Add(new ReportItem(index, getItemName(item), false, ex.Message));
                }
            }
        }

        return report.ToArray();
    }
}
