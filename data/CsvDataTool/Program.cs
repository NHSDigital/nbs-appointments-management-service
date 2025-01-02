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

        var itemTypeOption = new Option<string>
        (
            name: "--itemType",
            description: "The type of item being imported."
        );
        itemTypeOption.AddAlias("-it");

        var rootCommand = new RootCommand
        {
            inputPathOption,
            outputPathOption,
            reportPathOption,
            itemTypeOption
        };

        rootCommand.SetHandler(async (inputOptionValue, outputOptionValue, reportPathOptionValue, itemTypeOptionValue) =>
        {
            Console.WriteLine("MYA bulk site data importer started");

            if (!inputOptionValue.Exists)
            {
                Console.Error.WriteLine($"Input file {inputOptionValue} does not exist. Terminating.");
                return;
            }

            var dataImportHandler = DataImportHandlerFactory.GetHandler(itemTypeOptionValue);

            Console.WriteLine($"Processing csv data from {inputOptionValue}");
            var report  = await dataImportHandler.ProcessFile(inputOptionValue, outputOptionValue);

            Console.WriteLine($"Writing full report to {reportPathOptionValue}");
            var reportWriter = new ReportWriter(reportPathOptionValue);
            reportWriter.Write(report, true);

            var summaryReportFileInfo = GenerateShortReportName(reportPathOptionValue);
            Console.WriteLine($"Writing summary report to {summaryReportFileInfo}");
            var summaryWriter = new ReportWriter(summaryReportFileInfo);
            summaryWriter.Write(report, false);

            Console.WriteLine($"Processed {report.Count()} rows.");
            Console.WriteLine($"Succeeded: {report.Count(r => r.Success)} rows.");

            if (report.Any(r => !r.Success))
            {
                Console.Error.WriteLine($"Failed: {report.Count(r => !r.Success)}");
            }
        },
            inputPathOption, outputPathOption, reportPathOption, itemTypeOption);

        await rootCommand.InvokeAsync(args);
    }    

    private static FileInfo GenerateShortReportName(FileInfo reportFileInfo)
    {
        var pathStub = Path.GetFileNameWithoutExtension(reportFileInfo.FullName);
        return new FileInfo($"{pathStub}_summary.md");
    }
}
