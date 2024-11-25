using MassTransit;
using Moq;
using Nhs.Appointments.Api.Consumers;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Api.Tests.Consumers;

public class BookingCancelledConsumerTests
{
    private BookingCancelledConsumer _sut;
    private readonly Mock<IBookingCancelledNotifier> _notifier = new();

    public BookingCancelledConsumerTests()
    {
        _sut = new BookingCancelledConsumer(_notifier.Object, TimeProvider.System);
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
        DateOnly date = new DateOnly(DateTime.Now.AddYears(1).Year, 1, 1);
        TimeOnly time = new TimeOnly(12, 15);

        _notifier.Setup(x => x.Notify(nameof(BookingCancelled), Service, Reference, Site, FirstName, date, time, Email, PhoneNumber)).Verifiable();
        var ctx = new Mock<ConsumeContext<BookingCancelled>>();
        ctx.SetupGet(x => x.Message).Returns(new BookingCancelled
        {
            FirstName = FirstName,
            From = new DateTime(date, time),
            Reference = Reference,
            Site = Site,
            Service = Service,
            ContactDetails = [
                new ContactItem{Type = ContactItemType.Email, Value = Email},
                new ContactItem{Type = ContactItemType.Phone, Value = PhoneNumber}
                ]
        });

        await _sut.Consume(ctx.Object);

        _notifier.Verify();
    }

    [Fact]
    public async Task DoesNotNotifyUserIfNoContactDetails()
    {
        const string Site = "site:some-site";
        const string FirstName = "joe";
        const string Reference = "booking-ref-1234";
        const string Service = "covid-19";
        DateOnly date = new DateOnly(DateTime.Now.AddYears(1).Year, 1, 1);
        TimeOnly time = new TimeOnly(12, 15);

        _notifier.Setup(x => x.Notify(nameof(BookingCancelled), Service, Reference, Site, FirstName, date, time, It.IsAny<string>(), It.IsAny<string>())).Verifiable(Times.Never);
        var ctx = new Mock<ConsumeContext<BookingCancelled>>();
        ctx.SetupGet(x => x.Message).Returns(new BookingCancelled
        {
            FirstName = FirstName,
            From = new DateTime(date, time),
            Reference = Reference,
            Site = Site,
            Service = Service
        });

        await _sut.Consume(ctx.Object);

        _notifier.Verify();
    }

    [Fact]
    public async Task DoesNotNotifyUserIfAppointmentIsInThePast()
    {
        const string Site = "site:some-site";
        const string Email = "test@tempuri.org";
        const string FirstName = "joe";
        const string PhoneNumber = "0123456789";
        const string Reference = "booking-ref-1234";
        const string Service = "covid-19";
        DateOnly date = new DateOnly(DateTime.Now.AddYears(-1).Year, 1, 1);
        TimeOnly time = new TimeOnly(12, 15);

        _notifier.Setup(x => x.Notify(nameof(BookingCancelled), Service, Reference, Site, FirstName, date, time, Email, PhoneNumber)).Verifiable(Times.Never);
        var ctx = new Mock<ConsumeContext<BookingCancelled>>();
        ctx.SetupGet(x => x.Message).Returns(new BookingCancelled
        {
            FirstName = FirstName,
            From = new DateTime(date, time),
            Reference = Reference,
            Site = Site,
            Service = Service,
            ContactDetails = [
                new ContactItem{Type = ContactItemType.Email, Value = Email},
                new ContactItem{Type = ContactItemType.Phone, Value = PhoneNumber}
                ]
        });

        await _sut.Consume(ctx.Object);

        _notifier.Verify();
    }
}
