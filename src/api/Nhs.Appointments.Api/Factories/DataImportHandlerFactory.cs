using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Constants;
using System;

namespace Nhs.Appointments.Api.Factories;
public class DataImportHandlerFactory(IServiceProvider serviceProvider) : IDataImportHandlerFactory
{
    public IDataImportHandler CreateDataImportHandler(string importType) => importType switch
    {
        BulkImportType.ApiUser => serviceProvider.GetService<IApiUserDataImportHandler>(),
        BulkImportType.Site => serviceProvider.GetService<ISiteDataImportHandler>(),
        BulkImportType.User => serviceProvider.GetService<IUserDataImportHandler>(),
        _ => throw new NotSupportedException()
    };
}
