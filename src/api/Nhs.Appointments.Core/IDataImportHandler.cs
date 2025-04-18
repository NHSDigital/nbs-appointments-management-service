using Microsoft.AspNetCore.Http;

namespace Nhs.Appointments.Core;

public interface IDataImportHandler
{
    Task<IEnumerable<ReportItem>> ProcessFile(IFormFile inputFile);
}

public interface IApiUserDataImportHandler : IDataImportHandler { }
public interface ISiteDataImportHandler : IDataImportHandler { }
public interface IUserDataImportHandler : IDataImportHandler { }
