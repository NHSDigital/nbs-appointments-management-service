namespace DataExtract;
public class FileSenderOptions
{
    public string Type { get; set; } = "local";  // default
    public MeshSendOptions Mesh { get; set; } = new();
    public FileOptions Local { get; set; } = new();
    public BlobFileOptions Blob { get; set; } = new();
}
