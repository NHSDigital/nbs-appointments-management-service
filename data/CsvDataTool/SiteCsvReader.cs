using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace CsvDataTool;

public class SiteCsvReader
{
    private readonly FileInfo? _inputFile;
    private readonly string? _csvContent;
    private readonly Lazy<TextReader> _textReader;

    public SiteCsvReader(FileInfo inputFile)
    {
        _inputFile = inputFile;
        _textReader = new Lazy<TextReader>(() => _inputFile.OpenText());
    }

    public SiteCsvReader(string csvContent)
    {
        _csvContent = csvContent;
        _textReader = new Lazy<TextReader>(() => new StringReader(_csvContent));
    }

    public (Site[], SiteRowReportItem[]) Read()
    {
        int index = 0;
        var sites = new List<Site>();
        var report = new List<SiteRowReportItem>();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
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

            var imported = csv.GetRecords<Site>();
            foreach(var site in imported)
            {
                sites.Add(site);
                report.Add(new SiteRowReportItem(index, site.Name, true, ""));
                index++;
            }       
        }

        return (sites.ToArray(), report.ToArray());
    }
}
