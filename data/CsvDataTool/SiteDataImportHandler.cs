using CsvDataTool.Validators;
using Nhs.Appointments.Persistance.Models;

namespace CsvDataTool;

public class SiteDataImportHandler(IFileOperations fileOperations) : IDataImportHandler
{
    public Task<IEnumerable<ReportItem>> ProcessFile(FileInfo inputFile, DirectoryInfo outputFolder)
    {
        var processor = new CsvProcessor<SiteDocument, SiteMap>(s => WriteSiteDocument(s, outputFolder), s => s.Name,
            new SiteImportFileValidator());
        using var fileReader = fileOperations.OpenText(inputFile);
        return processor.ProcessFile(fileReader);
    }

    private Task WriteSiteDocument(SiteDocument siteDocument, DirectoryInfo outputDirectory)
    {
        fileOperations.CreateFolder(outputDirectory);
        var filePath = Path.Combine(outputDirectory.FullName, $"site_{siteDocument.Id}.json");
        return fileOperations.WriteDocument(siteDocument, filePath);
    }
}
