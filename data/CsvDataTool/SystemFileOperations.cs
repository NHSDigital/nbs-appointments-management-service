
using Nhs.Appointments.Persistance.Models;

namespace CsvDataTool;

public class SystemFileOperations : IFileOperations
{
    public void CreateFolder(DirectoryInfo folder) => folder.Create();
    public TextReader OpenText(FileInfo inputFile) => inputFile.OpenText();
    public Task WriteDocument<TDocument>(TDocument document, string path) where TDocument : TypedCosmosDocument => DocumentJsonWriter.Write<TDocument>(document, path);
}
