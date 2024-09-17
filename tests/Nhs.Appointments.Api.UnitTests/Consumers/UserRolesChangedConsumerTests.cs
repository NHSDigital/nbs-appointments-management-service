using MassTransit;
using Moq;
using Nhs.Appointments.Api.Consumers;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Api.Tests.Consumers;

public class UserRolesChangedConsumerTests
{
    private UserRolesChangedConsumer _sut;
    private readonly Mock<IUserRolesChangedNotifier> _notifier = new();
    public UserRolesChangedConsumerTests()
    {
        _sut = new UserRolesChangedConsumer(_notifier.Object);
    }

    [Fact]
    public async Task NotifiesUserOnEventReceipt()
    {
        const string user = "test@tempuri.org";
        const string site = "site1";
        string[] rolesAdded = ["role1"];
        string[] rolesRemoved = ["role2"];
        _notifier.Setup(x => x.Notify(user, site, It.Is<string[]>(r => Enumerable.SequenceEqual(r, rolesAdded)), It.Is<string[]>(r => Enumerable.SequenceEqual(r, rolesRemoved)))).Verifiable();
        var ctx = new Mock<ConsumeContext<UserRolesChanged>>();
        ctx.SetupGet(x => x.Message).Returns(new UserRolesChanged { UserId = user, SiteId = site, AddedRoleIds = rolesAdded, RemovedRoleIds = rolesRemoved });

        await _sut.Consume(ctx.Object);

        _notifier.Verify();
    }
}

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

        _notifier.Setup(x => x.Notify(Reference, Site, FirstName, date, time, EmailContactConsent, Email, PhoneContactConsent, PhoneNumber)).Verifiable();
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
