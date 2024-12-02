﻿using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace CsvDataTool;

public class SiteCsvReader
{
    private readonly FileInfo _inputFile;
    private readonly bool _hasHeaderRow;
    private readonly string _csvContent;

    public SiteCsvReader(FileInfo inputFile, bool hasHeaderRow)
    {
        _inputFile = inputFile;
        _hasHeaderRow = hasHeaderRow;

        if (!hasHeaderRow)
        {
            throw new ArgumentException("Headerless CSV files are not supported in this version of the tool");
        }
    }

    public SiteCsvReader(string csvContent, bool hasHeaderRow)
    {
        _csvContent = csvContent;
        _hasHeaderRow = hasHeaderRow;

        if(!hasHeaderRow)
        {
            throw new ArgumentException("Headerless CSV files are not supported in this version of the tool");
        }
    }

    public (Site[], SiteRowReportItem[]) Read()
    {
        int index = 0;
        var sites = new List<Site>();
        var report = new List<SiteRowReportItem>();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = _hasHeaderRow,
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

        using (var reader = GetCsvStreamReader())
        using (var csv = new CsvReader(reader, config))
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

    private TextReader GetCsvStreamReader()
    {
        if (_inputFile != null)
        {
            return _inputFile.OpenText();
        }

        if (!string.IsNullOrEmpty(_csvContent))
        {
            return new StringReader(_csvContent);
        }

        throw new Exception("No CSV data has been provided");
    }
}
