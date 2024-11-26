using System;

namespace Nhs.Appointments.Api.Notifications;

public class NotificationException : Exception
{
    public NotificationException(string message) : base(message)
    {
    }

    public NotificationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

