using MassTransit;
using Moq;
using Nhs.Appointments.Api.Consumers;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Api.Tests.Consumers;

public class BookingMadeConsumerTests
{
    private BookingMadeConsumer _sut;
    private readonly Mock<IBookingMadeNotifier> _notifier = new();

    public BookingMadeConsumerTests()
    { 
        _sut = new BookingMadeConsumer(_notifier.Object);
    }

    [Fact]
    public async Task NotifiesUserOnEventReceipt()
    {
        const string Site = "site:some-site";
        const string Email = "test@tempuri.org";
        const string FirstName = "joe";
        const string PhoneNumber = "0123456789";
        const bool EmailContactConsent = true;
        const bool PhoneContactConsent = true;
        const string Reference = "booking-ref-1234";
        const string Service = "covid-19";
        DateOnly date = new DateOnly(2050, 1, 1);
        TimeOnly time = new TimeOnly(12, 15);

        _notifier.Setup(x => x.Notify(nameof(BookingMade), Service, Reference, Site, FirstName, date, time, EmailContactConsent, Email, PhoneContactConsent, PhoneNumber)).Verifiable();
        var ctx = new Mock<ConsumeContext<BookingMade>>();
        ctx.SetupGet(x => x.Message).Returns(new BookingMade 
        { 
            Email = Email, 
            FirstName = FirstName, 
            From = new DateTime(date, time), 
            EmailContactConsent = EmailContactConsent, 
            PhoneContactConsent = PhoneContactConsent,
            PhoneNumber = PhoneNumber,
            Reference = Reference,
            Site = Site,
            Service = Service
        });

        await _sut.Consume(ctx.Object);

        _notifier.Verify();
    }
}
