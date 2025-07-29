using Nhs.Appointments.Core.BulkImport;

namespace Nhs.Appointments.Api.Factories;
public interface IDataImportHandlerFactory
{
    IDataImportHandler CreateDataImportHandler(string importType);
}
