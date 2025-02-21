namespace DataExtract;
public interface IExtractor
{
    Task RunAsync(FileInfo outputFile);
}
