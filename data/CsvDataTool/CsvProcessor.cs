using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using FluentValidation;

namespace CsvDataTool;

public class CsvProcessor<TDocument, TMap>(
    Func<TDocument, Task> processRow,
    Func<TDocument, string> getItemName,
    IValidator<TDocument> validator)
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

            var hasLineError = false;
            foreach (var row in imported.Select((rowDocument, rowIndex) => new { rowDocument, rowIndex }))
            {
                var validationResult = await validator.ValidateAsync(row.rowDocument);
                if (validationResult.IsValid)
                {
                    continue;
                }

                // Account for the header row and the loop indexing from 0
                var fileLine = row.rowIndex + 2;

                report.AddRange(validationResult.Errors
                    .Select(error => $"Line {fileLine}: {error.ErrorMessage}")
                    .Select(errorMessage =>
                        new ReportItem(fileLine, getItemName(row.rowDocument), false, errorMessage)));
                hasLineError = true;
            }

            // Prevent any data being written if there's an error anywhere in the file
            if (hasLineError)
            {
                return report.ToArray();
            }

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
