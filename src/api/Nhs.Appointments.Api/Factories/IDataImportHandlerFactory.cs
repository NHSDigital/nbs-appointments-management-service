using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Factories;
public interface IDataImportHandlerFactory
{
    IDataImportHandler CreateDataImportHandler(string importType);
}
