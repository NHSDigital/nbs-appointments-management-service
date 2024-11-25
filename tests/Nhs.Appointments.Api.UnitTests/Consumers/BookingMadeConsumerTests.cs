using MassTransit;
using Moq;
using Nhs.Appointments.Api.Consumers;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core;
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
        const string Reference = "booking-ref-1234";
        const string Service = "covid-19";
        const string Landline = "00001234567";
        DateOnly date = new DateOnly(2050, 1, 1);
        TimeOnly time = new TimeOnly(12, 15);

        _notifier.Setup(x => x.Notify(nameof(BookingMade), Service, Reference, Site, FirstName, date, time, Email, PhoneNumber)).Verifiable();
        var ctx = new Mock<ConsumeContext<BookingMade>>();
        ctx.SetupGet(x => x.Message).Returns(new BookingMade 
        { 
            FirstName = FirstName, 
            From = new DateTime(date, time), 
            Reference = Reference,
            Site = Site,
            Service = Service,
            ContactDetails= [
                new ContactItem{Type = ContactItemType.Email, Value = Email},
                new ContactItem{Type = ContactItemType.Phone, Value = PhoneNumber},
                new ContactItem{Type = ContactItemType.Landline, Value = Landline},
                ]
        });

        await _sut.Consume(ctx.Object);

        _notifier.Verify();
    }
}
