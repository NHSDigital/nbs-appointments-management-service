using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Nhs.Appointments.Persistance.Models;

namespace CsvDataTool;

public class CsvProcessor<TDocument, TMap>(Func<TDocument, Task> processRow, Func<TDocument, string> getItemName)
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

            var imported = csv.GetRecords<TDocument>().ToList();

            //hacky way to generate a Site ID not provided in the CSV or mapping
            if (typeof(TDocument) == typeof(SiteDocument))
            {
                var siteDocs = imported as List<SiteDocument>;
                foreach (var siteDoc in siteDocs!)
                {
                    siteDoc.Id = $"{Guid.NewGuid()}";
                }

                imported = siteDocs as List<TDocument>;
            }

            foreach (var item in imported!)
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
