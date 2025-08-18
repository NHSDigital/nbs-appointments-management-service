using Microsoft.Extensions.Options;

namespace DataExtract;
public class LocalFileSender : IFileSender
{
    private readonly LocalFileOptions _options;

    public LocalFileSender(IOptions<LocalFileOptions> options)
    {
        _options = options.Value;
    }

    public Task SendFile(FileInfo file)
    {
        var target = Path.Combine(_options.TargetPath, file.Name);
        File.Copy(file.FullName, target, _options.Overwrite);
        Console.WriteLine($"Saved {file.Name} to {target}");
        return Task.CompletedTask;
    }
}
