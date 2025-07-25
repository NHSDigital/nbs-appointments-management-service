using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using Nhs.Appointments.Api.Consumers;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Api.Tests.Functions;
public class AggregateDailySiteSummaryFunctionTests
{
    private readonly Mock<IMessageReceiver> _receiverMock = new();
    private readonly AggregateDailySiteSummaryFunction _sut;

    public AggregateDailySiteSummaryFunctionTests()
    {
        _sut = new AggregateDailySiteSummaryFunction(
            _receiverMock.Object
        );
    }

    [Fact]
    public async Task AggregateDailySiteSummaryAsync_DoesTriggerConsumer()
    {
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromString("test"));
        var cancellationToken = CancellationToken.None;

        await _sut.AggregateDailySiteSummary(message, cancellationToken);

        _receiverMock.Verify(
            r => r.HandleConsumer<AggregateSiteSummaryConsumer>(
                It.IsAny<string>(), It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
