using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Api.Notifications.Options;
using Notify.Exceptions;
using Notify.Interfaces;
using Notify.Models.Responses;

namespace Nhs.Appointments.Api.Tests.Notifications;

public class GovNotifyClientTests
{
    /// <summary>
    ///     Example of GovNotify error format provided in https://docs.notifications.service.gov.uk/net.html#error-handling
    /// </summary>
    private const string ExampleErrorFromDocs =
        "Status code 403. Error: {\"errors\":[{\"error\":\"AuthError\",\"message\":\"Invalid token: API key not found\"}],\"status_code\":403}\n, Exception: Status code 403. The following errors occured [\n  {\n    \"error\": \"AuthError\",\n    \"message\": \"Invalid token: API key not found\"\n  }\n]";

    private readonly Mock<IAsyncNotificationClient> _notificationClient = new();
    private readonly Mock<ILogger<GovNotifyClient>> _logger = new();
    private readonly Mock<IPrivacyUtil> _privacyUtil = new();
    private readonly GovNotifyClient _sut;

    public GovNotifyClientTests()
    {
        var retryOptions = Options.Create(new GovNotifyRetryOptions
        {
            MaxRetries = 3, InitialDelayMs = 1, BackoffFactor = 1.0
        });

        _sut = new GovNotifyClient(_notificationClient.Object, _privacyUtil.Object, retryOptions, _logger.Object);
    }

    [Theory]
    [InlineData("Status code foo", null)]
    [InlineData(ExampleErrorFromDocs, 403)]
    [InlineData("Some text before Status code 422 some text after", 422)]
    public void CanParseErrorMessages(string error, int? expectedStatusCode)
    {
        var result = _sut.ParseGovNotifyExceptionMessage(error);

        result.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData("Status code foo", 1)]
    [InlineData("Status code 400", 1)]
    [InlineData("Status code 401", 1)]
    [InlineData("Status code 403", 1)]
    [InlineData("Status code 404", 1)]
    [InlineData("Status code 429", 3)]
    public async Task SendByEmail_RetriesByErrorStatusCode(string errorMessage, int expectedNumberOfTries)
    {
        _notificationClient.Setup(x => x.SendEmailAsync(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, dynamic>>(),
                null,
                null,
                null))
            .Throws(new NotifyClientException(errorMessage));

        await _sut.SendEmailAsync("test@tempuri.org", "email-template", new Dictionary<string, dynamic>());

        _notificationClient.Verify(
            x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, dynamic>>(),
                null,
                null,
                null
            ),
            Times.Exactly(expectedNumberOfTries)
        );
    }

    [Theory]
    [InlineData("Status code foo", 1)]
    [InlineData("Status code 400", 1)]
    [InlineData("Status code 401", 1)]
    [InlineData("Status code 403", 1)]
    [InlineData("Status code 404", 1)]
    [InlineData("Status code 429", 3)]
    public async Task SendBySms_RetriesByErrorStatusCode(string errorMessage, int expectedNumberOfTries)
    {
        _notificationClient.Setup(x => x.SendSmsAsync(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, dynamic>>(),
                null,
                null))
            .Throws(new NotifyClientException(errorMessage));

        await _sut.SendSmsAsync("1234567890", "email-template", new Dictionary<string, dynamic>());

        _notificationClient.Verify(
            x => x.SendSmsAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, dynamic>>(),
                null,
                null
            ),
            Times.Exactly(expectedNumberOfTries)
        );
    }

    [Fact(DisplayName = "Retries on NotifyClientException")]
    public async Task GovNotifyClient_Retries_On_NotifyClientException()
    {
        // Simulate: 2 failures then success
        _notificationClient
            .SetupSequence(x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, dynamic>>(),
                null,
                null,
                null
            ))
            .ThrowsAsync(new NotifyClientException("Status code 429 first fail"))
            .ThrowsAsync(new NotifyClientException("Status code 429 second fail"))
            .ReturnsAsync(new EmailNotificationResponse());

        // Act
        await _sut.SendEmailAsync("test@tempuri.org", "email-template", new Dictionary<string, dynamic>());

        // Assert
        _notificationClient.Verify(
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
