using Newtonsoft.Json;

namespace CosmosDbSeederTests;

public class BaseCosmosDbSeederTest
{
    protected static T ReadDocument<T>(string environment)
    {
        var documentName = GetDocumentName<T>();

        var document = JsonConvert.DeserializeObject<T>(File.ReadAllText(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            $"items/{environment}/core_data/{documentName}.json")));

        return document ??
               throw new Exception($"Could not read {documentName}.json for {environment} environment");
    }

    private static string GetDocumentName<T>()
    {
        var docName = typeof(T).Name;
        switch (typeof(T).Name)
        {
            case nameof(GlobalRolesDocument):
                return "global_roles";
            case nameof(ClinicalServicesDocument):
                return "clinical_services";
            case nameof(NotificationConfigDocument):
                return "notification_config";
            default:
                throw new Exception($"Could not read {typeof(T).Name} from {typeof(T).Name}");
        }
    }
}
