using Azure.Messaging.ServiceBus;
using MassTransit;
using Moq;
using Nhs.Appointments.Api.Consumers;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Api.Tests.Functions;
public class NotifyOktaUserRolesChangedFunctionTests
{
    private readonly Mock<IFeatureToggleHelper> _featureToggleHelperMock;
    private readonly Mock<IMessageReceiver> _receiverMock;
    private readonly NotifyOktaUserRolesChangedFunction _sut;

    public NotifyOktaUserRolesChangedFunctionTests()
    {
        _featureToggleHelperMock = new Mock<IFeatureToggleHelper>();
        _receiverMock = new Mock<IMessageReceiver>();

        _sut = new NotifyOktaUserRolesChangedFunction(
            _receiverMock.Object,
            _featureToggleHelperMock.Object
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
        await _sut.NotifyOktaUserRolesChangedAsync(message, cancellationToken);

        // Assert
        _receiverMock.Verify(
            r => r.HandleConsumer<OktaUserRolesChangedConsumer>(
                It.IsAny<string>(), It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
