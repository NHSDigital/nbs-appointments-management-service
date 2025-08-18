namespace DataExtract;

public interface IFileSender
{
    Task SendFile(FileInfo file);
}
