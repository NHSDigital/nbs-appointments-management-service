using CsvHelper;
using CsvHelper.Configuration;
using Nhs.Appointments.Persistance.Models;
using System.Globalization;

namespace CsvDataTool;

public class SiteCsvReader
{
    private readonly FileInfo _inputFile;
    private readonly string _csvContent;
    private readonly Lazy<TextReader> _textReader;

    public SiteCsvReader(FileInfo inputFile)
    {
        _inputFile = inputFile;
        _textReader = new Lazy<TextReader>(() => _inputFile.OpenText());
    }

    public async Task<IEnumerable<SiteRowReportItem>> ReadAndProcessAsync(Func<SiteDocument, Task> process)
    {
        int index = -1;
        var report = new List<SiteRowReportItem>();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            ShouldSkipRecord = args => { 
                index++;
                return false;
            }, 
            ReadingExceptionOccurred = args =>
            {
                report.Add(new SiteRowReportItem(index, "", false, args.Exception.ToString()));
                
                return false;
            },
            BadDataFound = args =>
            {
                report.Add(new SiteRowReportItem(index, "", false, $"Bad data found in field '{args.Field}'"));
            },
            Delimiter = ",",
            Quote = '"'
        };

        using (_textReader.Value)
        using (var csv = new CsvReader(_textReader.Value, config))
        {
            csv.Context.RegisterClassMap<SiteMap>();

            var imported = csv.GetRecords<SiteDocument>();
            foreach(var site in imported)
            {
                try
                {
                    await process(site);
                    report.Add(new SiteRowReportItem(index, site.Name, true, ""));
                }
                catch (Exception ex)
                {
                    report.Add(new SiteRowReportItem(index, site.Name, false, ex.Message));
                }
            }       
        }

        return report.ToArray();
    }
}
