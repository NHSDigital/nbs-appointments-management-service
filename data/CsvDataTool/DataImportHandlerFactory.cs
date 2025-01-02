namespace CsvDataTool;

public static class DataImportHandlerFactory
{
    public static IDataImportHandler GetHandler(string itemType) => itemType switch
    {
        "site" => new SiteDataImportHandler(new SystemFileOperations()),
        "user" => new UserDataImportHandler(new SystemFileOperations()),
        "apiUser" => new ApiUserDataImportHandler(new SystemFileOperations()),
        _ => throw new NotSupportedException($"Import of {itemType} items is not supported")
    };
}
