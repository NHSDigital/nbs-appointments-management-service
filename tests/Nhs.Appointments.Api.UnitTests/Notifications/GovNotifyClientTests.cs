using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Notify.Client;
using Notify.Exceptions;
using Nhs.Appointments.Api.Notifications;
using Notify.Models.Responses;
using Notify.Interfaces;
using Nhs.Appointments.Api.Notifications.Options;

namespace Nhs.Appointments.Api.Tests.Notifications;

public class GovNotifyClientTests
{
    [Fact(DisplayName = "Retries on NotifyClientException")]
    public async Task GovNotifyClient_Retries_On_NotifyClientException()
    {
        // Arrange
        var mockClient = new Mock<IAsyncNotificationClient>();

        // Simulate: 2 failures then success
        mockClient
            .SetupSequence(x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, dynamic>>(),
                null,
                null,
                null
            ))
            .ThrowsAsync(new NotifyClientException("first fail"))
            .ThrowsAsync(new NotifyClientException("second fail"))
            .ReturnsAsync(new EmailNotificationResponse());

        var retryOptions = Options.Create(new GovNotifyRetryOptions
        {
            MaxRetries = 3,
            InitialDelayMs = 1,
            BackoffFactor = 1.0
        });

        var privacyUtil = Mock.Of<IPrivacyUtil>();
        var client = new GovNotifyClient(mockClient.Object, privacyUtil, retryOptions);

        // Act
        await client.SendEmailAsync("test@tempuri.org", "email-template", new());

        // Assert
        mockClient.Verify(
                x => x.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, dynamic>>(),
                    null,
                    null,
                    null
                ),
                Times.Exactly(3)
            );
    }
}
