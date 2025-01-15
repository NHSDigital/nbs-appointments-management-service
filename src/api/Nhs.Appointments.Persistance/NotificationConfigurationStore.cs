using AutoMapper;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class NotificationConfigurationStore(ITypedDocumentCosmosStore<NotificationConfigurationDocument> cosmosStore, IMapper mapper) : INotificationConfigurationStore
{
    private const string DocumentId = "notification_configuration";

    public async Task<IEnumerable<NotificationConfiguration>> GetNotificationConfiguration()
    {
        var globalDocument = await GetConfig();

        return globalDocument.Configs.Select(mapper.Map<Core.NotificationConfiguration>);        
    }        

    private async Task<NotificationConfigurationDocument> GetConfig()
    {
        var globalDocument = await cosmosStore.GetByIdAsync<NotificationConfigurationDocument>(DocumentId);
        if(globalDocument == null)
        {
            throw new Exception("The notification configuration document could not be retrieved");
        }

        return globalDocument;
    }
}
