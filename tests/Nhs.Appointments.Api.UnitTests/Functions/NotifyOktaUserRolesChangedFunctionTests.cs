using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using Nhs.Appointments.Api.Consumers;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Api.Tests.Functions;
public class NotifyOktaUserRolesChangedFunctionTests
{
    private readonly Mock<IFeatureToggleHelper> _featureToggleHelperMock = new();
    private readonly Mock<IMessageReceiver> _receiverMock = new();
    private readonly Mock<ILogger<NotifyOktaUserRolesChangedFunction>> _loggerMock = new();
    private readonly Mock<ServiceBusMessageActions> _messageActionsMock = new();
    private readonly NotifyOktaUserRolesChangedFunction _sut;

    public NotifyOktaUserRolesChangedFunctionTests()
    {
        _sut = new NotifyOktaUserRolesChangedFunction(
            _receiverMock.Object,
            _featureToggleHelperMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task NotifyOktaUserRolesChangedAsync_DoesNotTriggerConsumer_WhenOktaIsDisabled()
    {
        // Arrange
        _featureToggleHelperMock
            .Setup(x => x.IsFeatureEnabled(Flags.OktaEnabled))
            .ReturnsAsync(false);

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromString("test"));
        var cancellationToken = CancellationToken.None;

        // Act
        await _sut.NotifyOktaUserRolesChangedAsync(message, _messageActionsMock.Object, cancellationToken);

        // Assert
        _receiverMock.Verify(
            r => r.HandleConsumer<OktaUserRolesChangedConsumer>(
                It.IsAny<string>(), It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _messageActionsMock.Verify(
            m => m.DeadLetterMessageAsync(
                message,
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task NotifyOktaUserRolesChangedAsync_DoesTriggerConsumer_WhenOktaIsEnabled()
    {
        // Arrange
        _featureToggleHelperMock
            .Setup(x => x.IsFeatureEnabled(Flags.OktaEnabled))
            .ReturnsAsync(true);

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromString("test"));
        var cancellationToken = CancellationToken.None;

        // Act
        await _sut.NotifyOktaUserRolesChangedAsync(message, _messageActionsMock.Object, cancellationToken);

        // Assert
        _receiverMock.Verify(
            r => r.HandleConsumer<OktaUserRolesChangedConsumer>(
                It.IsAny<string>(), It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _messageActionsMock.Verify(
            m => m.DeadLetterMessageAsync(
                message,
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                cancellationToken),
            Times.Never);
    }
}
