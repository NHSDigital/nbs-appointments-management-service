using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataExtract;
public interface IFileSenderFactory
{
    IFileSender Create(string type);
}

public class FileSenderFactory : IFileSenderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public FileSenderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IFileSender Create(string type) => type.ToLower() switch
    {
        "mesh" => _serviceProvider.GetRequiredService<MeshFileSender>(),
        "local" => _serviceProvider.GetRequiredService<LocalFileSender>(),
        "blob" => _serviceProvider.GetRequiredService<BlobFileSender>(),
        _ => throw new ArgumentException($"Unknown sender type: {type}")
    };
}
