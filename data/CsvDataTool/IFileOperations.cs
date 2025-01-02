using Nhs.Appointments.Persistance.Models;

namespace CsvDataTool;

public interface IFileOperations
{
    TextReader OpenText(FileInfo inputFile);
    void CreateFolder(DirectoryInfo folder);
    Task WriteDocument<TDocument>(TDocument document, string path) where TDocument : TypedCosmosDocument;
}
