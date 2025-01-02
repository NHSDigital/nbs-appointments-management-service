namespace CsvDataTool;

public interface IDataImportHandler
{
    Task<IEnumerable<ReportItem>> ProcessFile(FileInfo inputFile, DirectoryInfo outputFolder);
}
