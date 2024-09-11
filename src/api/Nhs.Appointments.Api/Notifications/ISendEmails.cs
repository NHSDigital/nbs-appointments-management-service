using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications;

public interface ISendEmails
{
    Task SendEmailAsync(string emailAddress, string templateId, Dictionary<string, dynamic> templateValues);
}
