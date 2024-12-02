using System.CommandLine;

namespace CsvDataTool;

public class Program
{
    static async Task Main(string[] args)
    {
        var inputPathOption = new Option<FileInfo>
        (
            name: "--in",
            description: "The location of the file to parse.",
            getDefaultValue: () => new FileInfo("sites.csv"));
        inputPathOption.AddAlias("-i");

        var outputPathOption = new Option<FileInfo>
        (
            name: "--out",
            description: "The output location for the json file.",
            getDefaultValue: () => new FileInfo("sites.json"));
        outputPathOption.AddAlias("-o");

        var reportPathOption = new Option<FileInfo>
        (
            name: "--report",
            description: "The output location for the report html file.",
            getDefaultValue: () => new FileInfo("report.html"));
        reportPathOption.AddAlias("-r");

        var hasHeaderRowOption = new Option<bool>
        (
            name: "--has-header-row",
            description: "Set this to true if the first row of the CSV file contains column names.",
            getDefaultValue: () => true);
        hasHeaderRowOption.AddAlias("-h");

        var rootCommand = new RootCommand();
        rootCommand.Add(inputPathOption);
        rootCommand.Add(outputPathOption);
        rootCommand.Add(reportPathOption);
        rootCommand.Add(hasHeaderRowOption);

        rootCommand.SetHandler(async (inputOptionValue, outputOptionValue, reportPathOptionValue, hasHeaderRowOptionValue) =>
        {
            Console.WriteLine("MYA bulk site data importer started");

            if (!inputOptionValue.Exists)
            {
                Console.Error.WriteLine($"Input file {inputOptionValue} does not exist. Terminating.");
                return;
            }

            if (!hasHeaderRowOptionValue)
            {
                Console.Error.WriteLine("This version of the tool does not support csv files without a header row.");
                return;
            }

            Console.WriteLine($"Processing csv data from {inputOptionValue}");
            var reader = new SiteCsvReader(inputOptionValue, hasHeaderRowOptionValue);
            var (sites, report) = reader.Read();

            Console.WriteLine($"Writing sites json to {outputOptionValue}");
            var writer = new SiteJsonWriter(outputOptionValue);
            await writer.Write(sites);

            Console.WriteLine($"Writing report to {reportPathOptionValue}");
            var reportWriter = new SiteReportWriter(reportPathOptionValue);
            await reportWriter.Write(report);

            Console.WriteLine($"Processed {report.Length} rows.");
            Console.WriteLine($"Succeeded: {sites.Length}");

            if (report.Any(r => !r.Success))
            {
                Console.Error.WriteLine($"Failed: {report.Count(r => !r.Success)}");
            }
        },
            inputPathOption, outputPathOption, reportPathOption, hasHeaderRowOption);

        await rootCommand.InvokeAsync(args);
    }
}

