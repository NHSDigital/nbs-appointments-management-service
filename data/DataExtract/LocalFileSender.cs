using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataExtract;
public class LocalFileSender : IFileSender
{
    private readonly LocalFileOptions _options;
    private readonly ILogger<LocalFileSender> _logger;

    public LocalFileSender(IOptions<LocalFileOptions> options, ILogger<LocalFileSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task SendFile(FileInfo file)
    {
        var target = Path.Combine(_options.TargetPath, file.Name);

        Directory.CreateDirectory(_options.TargetPath);

        File.Copy(file.FullName, target, _options.Overwrite);
        _logger.LogInformation($"Saved {file.Name} to {target}");
        return Task.CompletedTask;
    }
}
