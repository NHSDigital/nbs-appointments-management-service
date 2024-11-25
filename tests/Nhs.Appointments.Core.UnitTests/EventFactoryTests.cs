using Nhs.Appointments.Core.Messaging;

namespace Nhs.Appointments.Core.UnitTests
{
    public class EventFactoryTests
    {
        private readonly EventFactory _sut = new EventFactory();

        [Fact]
        public void BuildsBookingMadeEventCorrectly()
        {
            var booking = BuildBooking();
            var e = _sut.BuildBookingMadeEvent(booking);
            e.FirstName.Should().Be(booking.AttendeeDetails.FirstName);
            e.LastName.Should().Be(booking.AttendeeDetails.LastName);
            e.From.Should().Be(booking.From);
            e.Reference.Should().Be(booking.Reference);
            e.Service.Should().Be(booking.Service);
            e.Site.Should().Be(booking.Site);
            e.ContactDetails.Should().NotBeNull();
            e.ContactDetails.Should().Contain(c => c.Type == ContactItemType.Email && c.Value == "test@tempuri.org");
            e.ContactDetails.Should().Contain(c => c.Type == ContactItemType.Phone && c.Value == "1234567890");
        }

        [Fact]
        public void BookingMadeEventMustContainContactDetails()
        {
            var booking = BuildBooking(false);
            Assert.Throws<ArgumentException>(() => _sut.BuildBookingMadeEvent(booking)).ParamName.Should().Be(nameof(booking.ContactDetails));
        }

        [Fact]
        public void BuildsBookingRescheduledEventCorrectly()
        {
            var booking = BuildBooking();
            var e = _sut.BuildBookingRescheduledEvent(booking);
            e.FirstName.Should().Be(booking.AttendeeDetails.FirstName);
            e.LastName.Should().Be(booking.AttendeeDetails.LastName);
            e.From.Should().Be(booking.From);
            e.Reference.Should().Be(booking.Reference);
            e.Service.Should().Be(booking.Service);
            e.Site.Should().Be(booking.Site);
            e.ContactDetails.Should().NotBeNull();
            e.ContactDetails.Should().Contain(c => c.Type == ContactItemType.Email && c.Value == "test@tempuri.org");
            e.ContactDetails.Should().Contain(c => c.Type == ContactItemType.Phone && c.Value == "1234567890");
        }

        [Fact]
        public void BookingRescheduledEventMustContainContactDetails()
        {
            var booking = BuildBooking(false);
            Assert.Throws<ArgumentException>(() => _sut.BuildBookingRescheduledEvent(booking)).ParamName.Should().Be(nameof(booking.ContactDetails));
        }

        [Fact]
        public void BuildsBookingCancelledEventCorrectly()
        {
            var booking = BuildBooking();
            var e = _sut.BuildBookingCancelledEvent(booking);
            e.FirstName.Should().Be(booking.AttendeeDetails.FirstName);
            e.LastName.Should().Be(booking.AttendeeDetails.LastName);
            e.From.Should().Be(booking.From);
            e.Reference.Should().Be(booking.Reference);
            e.Service.Should().Be(booking.Service);
            e.Site.Should().Be(booking.Site);
            e.ContactDetails.Should().NotBeNull();
            e.ContactDetails.Should().Contain(c => c.Type == ContactItemType.Email && c.Value == "test@tempuri.org");
            e.ContactDetails.Should().Contain(c => c.Type == ContactItemType.Phone && c.Value == "1234567890");
        }

        [Fact]
        public void BuildsBookingReminderEventCorrectly()
        {
            var booking = BuildBooking();
            var e = _sut.BuildBookingReminderEvent(booking);
            e.FirstName.Should().Be(booking.AttendeeDetails.FirstName);
            e.LastName.Should().Be(booking.AttendeeDetails.LastName);
            e.From.Should().Be(booking.From);
            e.Reference.Should().Be(booking.Reference);
            e.Service.Should().Be(booking.Service);
            e.Site.Should().Be(booking.Site);
            e.ContactDetails.Should().NotBeNull();
            e.ContactDetails.Should().Contain(c => c.Type == ContactItemType.Email && c.Value == "test@tempuri.org");
            e.ContactDetails.Should().Contain(c => c.Type == ContactItemType.Phone && c.Value == "1234567890");
        }

        [Fact]
        public void BookingReminderEventMustContainContactDetails()
        {
            var booking = BuildBooking(false);
            Assert.Throws<ArgumentException>(() => _sut.BuildBookingReminderEvent(booking)).ParamName.Should().Be(nameof(booking.ContactDetails));
        }


        private static Booking BuildBooking(bool withContact = true)
        {
            ContactItem[]? contact = withContact ? [ new ContactItem { Type = ContactItemType.Email, Value = "test@tempuri.org"}, new ContactItem { Type = ContactItemType.Phone, Value = "1234567890"}] : null;
            return new Booking
            {
                AttendeeDetails = new AttendeeDetails
                {
                    FirstName = "firstname",
                    LastName = "lastname"
                },
                From = new DateTime(2024, 11, 14),
                Reference = "reference",
                Service = "service",
                Site = "site",
                ContactDetails = contact
            };
        }
    }
}
