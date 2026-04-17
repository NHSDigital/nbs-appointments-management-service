using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class NotificationConfigurationStore(ITypedDocumentCosmosStore<NotificationConfigurationDocument> cosmosStore) : INotificationConfigurationStore
{
    private const string DocumentId = "notification_configuration";

    public async Task<IEnumerable<NotificationConfiguration>> GetNotificationConfiguration()
    {
        var globalDocument = await GetConfig();

        return globalDocument.Configs.Select(config => new NotificationConfiguration
        {
            EmailTemplateId = config.EmailTemplateId,
            EventType = config.EventType,
            Services = config.Services,
            SmsTemplateId = config.SmsTemplateId,
        });
    }

    private async Task<NotificationConfigurationDocument> GetConfig()
    {
        var globalDocument = await cosmosStore.GetByIdAsync(DocumentId);
        if(globalDocument == null)
        {
            throw new Exception("The notification configuration document could not be retrieved");
        }

        return globalDocument;
    }
}
