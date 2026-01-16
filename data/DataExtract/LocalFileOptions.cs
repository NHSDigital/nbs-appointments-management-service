namespace DataExtract;
public class LocalFileOptions
{
    public required string TargetPath { get; set; }
    public bool Overwrite { get; set; } = true;
}
