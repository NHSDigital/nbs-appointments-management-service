using MassTransit;
using Moq;
using Nhs.Appointments.Api.Consumers;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Api.Tests.Consumers;
public class BookingConsumerTests
{
    private readonly Mock<IBookingNotifier> _notifier = new();

    [Fact]
    public async Task Consume_CallsNotify_WhenNoticationValidAndCorrect()
    {
        var date = new DateOnly(2050, 1, 1);
        var time = new TimeOnly(12, 15);
        _notifier.Setup(x => x.Notify(nameof(BookingMade), "Service", "Reference", "Site", "FirstName", date, time, NotificationType.Email, "test@test.com")).Verifiable();
        var sut = new TestBookingConsumer(_notifier.Object);
        var ctx = new Mock<ConsumeContext<BookingMade>>();
        ctx.SetupGet(x => x.Message).Returns(new BookingMade
        {
            FirstName = "FirstName",
            LastName = "LastName",
            From = new DateTime(date, time),
            Reference = "Reference",
            Site = "Site",
            Service = "Service",
            Destination = "test@test.com",
            NotificationType = NotificationType.Email
        });
        await sut.Consume(ctx.Object);
    }
}

internal class TestBookingConsumer(IBookingNotifier bookingNotifier) : BookingConsumer<BookingMade>(bookingNotifier)
{

}
