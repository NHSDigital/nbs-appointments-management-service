using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Core.BulkImport;
using Nhs.Appointments.Core.Constants;
using System;

namespace Nhs.Appointments.Api.Factories;
public class DataImportHandlerFactory(IServiceProvider serviceProvider) : IDataImportHandlerFactory
{
    public IDataImportHandler CreateDataImportHandler(string importType) => importType switch
    {
        BulkImportType.AdminUser => serviceProvider.GetService<IAdminUserDataImportHandler>(),
        BulkImportType.ApiUser => serviceProvider.GetService<IApiUserDataImportHandler>(),
        BulkImportType.Site => serviceProvider.GetService<ISiteDataImportHandler>(),
        BulkImportType.User => serviceProvider.GetService<IUserDataImportHandler>(),
        BulkImportType.SiteStatus => serviceProvider.GetService<ISiteStatusDataImportHandler>(),
        _ => throw new NotSupportedException()
    };
}
