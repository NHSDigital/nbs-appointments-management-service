using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Nhs.Appointments.Core;

public class CsvProcessor<TDocument, TMap>(
    Func<TDocument, Task> processRow,
    Func<TDocument, string> getItemName,
    Func<TMap> createMap)
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
                var exMsg = args.Exception.InnerException is null
                    ? args.Exception.ToString()
                    : args.Exception.InnerException.Message.ToString();

                report.Add(new ReportItem(index, "", false, exMsg));

                return false;
            },
            BadDataFound = args =>
            {
                report.Add(new ReportItem(index, "", false, $"Bad data found in field '{args.Field}'"));
            },
            Delimiter = ",",
            Quote = '"',
            TrimOptions = TrimOptions.Trim
        };

        using (var csv = new CsvReader(csvReader, config))
        {
            csv.Context.RegisterClassMap(createMap());

            try
            {
                var imported = csv.GetRecords<TDocument>();

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
            catch (Exception ex)
            {
                report.Add(new ReportItem(index, "Error trying to parse CSV file.", false, $"Error trying to parse CSV file: {ex.Message}"));
            }
        }

        return report.ToArray();
    }
}
