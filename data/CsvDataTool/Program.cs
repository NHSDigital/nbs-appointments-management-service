using System.CommandLine;
using Nhs.Appointments.Persistance.Models;

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

        var outputPathOption = new Option<DirectoryInfo>
        (
            name: "--out",
            description: "The output location for the json file.",
            getDefaultValue: () => new DirectoryInfo("sites"));
        outputPathOption.AddAlias("-o");

        var reportPathOption = new Option<FileInfo>
        (
            name: "--report",
            description: "The output location for the report markdown file.",
            getDefaultValue: () => new FileInfo("csv_conversion_report.md"));
        reportPathOption.AddAlias("-r");

        var rootCommand = new RootCommand();
        rootCommand.Add(inputPathOption);
        rootCommand.Add(outputPathOption);
        rootCommand.Add(reportPathOption);

        rootCommand.SetHandler(async (inputOptionValue, outputOptionValue, reportPathOptionValue) =>
        {
            Console.WriteLine("MYA bulk site data importer started");

            if (!inputOptionValue.Exists)
            {
                Console.Error.WriteLine($"Input file {inputOptionValue} does not exist. Terminating.");
                return;
            }

            Console.WriteLine($"Processing csv data from {inputOptionValue}");
            var reader = new SiteCsvReader(inputOptionValue);
            var report = await reader.ReadAndProcessAsync(s => WriteSiteDocument(s, outputOptionValue));
            
            Console.WriteLine($"Writing full report to {reportPathOptionValue}");
            var reportWriter = new SiteReportWriter(reportPathOptionValue);
            reportWriter.Write(report);
            
            var summaryReportFileInfo = GenerateShortReportName(reportPathOptionValue);
            Console.WriteLine($"Writing summary report to {summaryReportFileInfo}");
            var summaryWriter = new SiteReportWriter(summaryReportFileInfo);
            summaryWriter.Write(report);

            Console.WriteLine($"Processed {report.Count()} rows.");
            Console.WriteLine($"Succeeded: {report.Count(r => r.Success)} rows.");

            if (report.Any(r => !r.Success))
            {
                Console.Error.WriteLine($"Failed: {report.Count(r => !r.Success)}");
            }
        },
            inputPathOption, outputPathOption, reportPathOption);

        await rootCommand.InvokeAsync(args);
    }

    private static Task WriteSiteDocument(SiteDocument siteDocument, DirectoryInfo outputDirectory)
    {
        outputDirectory.Create();
        var filePath = Path.Combine(outputDirectory.FullName, $"site_{siteDocument.Id}.json");
        return SiteJsonWriter.Write(siteDocument, filePath);
    }

    private static FileInfo GenerateShortReportName(FileInfo reportFileInfo)
    {
        var pathStub = Path.GetFileNameWithoutExtension(reportFileInfo.FullName);
        return new FileInfo($"{pathStub}_summary.md");
    }
}
