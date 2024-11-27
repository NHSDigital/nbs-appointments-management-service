using FluentAssertions;
using Nhs.Appointments.Api.Notifications;
using Notify.Exceptions;

namespace Nhs.Appointments.Api.Tests.Notifications;

public class NotificationExceptionTests
{
    [Fact]
    public void IncludesGovNotifyMessage()
    {
        var govNotifyErrorReason = "The number was not whitelisted";
        var govNotifyException = new NotifyClientException(govNotifyErrorReason);
        var ourException = new NotificationException("test", govNotifyException);

        ourException.Message.Should().Contain($"Reason: {govNotifyErrorReason}");
    }
}
