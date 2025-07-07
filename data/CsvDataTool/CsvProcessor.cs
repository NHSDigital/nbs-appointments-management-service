using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using FluentValidation;
using FluentValidation.Results;

namespace CsvDataTool;

public class CsvProcessor<TDocument, TMap>(
    Func<TDocument, Task> processRow,
    Func<TDocument, string> getItemName,
    IValidator<List<TDocument>> validator)
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

        var importFile = ReadCsv(csvReader, config);
        if (report.Any(r => r.Success == false))
        {
            Console.WriteLine("There were errors while reading the file. No rows will be written.");
            return report.ToArray();
        }

        var validatedFile = await validator.ValidateAsync(importFile);

        if (!validatedFile.IsValid)
        {
            foreach (var error in validatedFile.Errors)
            {
                var errorInfo = ExtractReportInfo(error);

                report.Add(new ReportItem(errorInfo.lineNumber, errorInfo.propertyName, false,
                    errorInfo.errorMessageForLine));
            }

            Console.WriteLine("There were validation errors within the file. No rows will be written.");
            return report.ToArray();
        }

        foreach (var item in importFile)
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

        return report.ToArray();
    }

    private List<TDocument> ReadCsv(TextReader csvReader, CsvConfiguration config)
    {
        using var csv = new CsvReader(csvReader, config);
        csv.Context.RegisterClassMap<TMap>();
        return csv.GetRecords<TDocument>().ToList();
    }

    private (int lineNumber, string propertyName, string errorMessageForLine)
        ExtractReportInfo(ValidationFailure error)
    {
        var extractedPropertyName =
            error.PropertyName.Contains('.') ? error.PropertyName.Split('.').Last() : error.PropertyName;
        if (!error.ErrorMessage.Contains(':'))
        {
            return (-1, extractedPropertyName, error.ErrorMessage);
        }

        var lineNumber =
            int.TryParse(error.ErrorMessage.Split(':')[0], out var result)
                ? result + 2 // + 2 to account for the header line and indexing from 0
                : -1;
        var errorMessageForLine = error.ErrorMessage.Split(':')[1][1..];

        return (lineNumber, extractedPropertyName, errorMessageForLine);
    }
}
