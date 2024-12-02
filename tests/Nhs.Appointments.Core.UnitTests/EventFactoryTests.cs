using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Core.UnitTests
{
    public class EventFactoryTests
    {
        private readonly EventFactory _sut = new EventFactory();

        [Fact]
        public void BuildsBookingMadeEventsCorrectly()
        {
            BuildsEventCorrectly<BookingMade>();
        }

        [Fact]
        public void BuildsBookingRescheduledEventCorrectly()
        {
            BuildsEventCorrectly<BookingRescheduled>();
        }

        [Fact]
        public void BuildsBookingCancelledEventCorrectly()
        {
            BuildsEventCorrectly<BookingCancelled>();
        }

        [Fact]
        public void BuildsBookingReminderEventCorrectly()
        {
            BuildsEventCorrectly<BookingReminder>();
        }

        [Fact]
        public void BookingReminderEventMustContainContactDetails()
        {
            EventMustContainContactDetails<BookingReminder>();
        }

        [Fact]
        public void BookingRescheduledEventMustContainContactDetails()
        {
            EventMustContainContactDetails<BookingRescheduled>();
        }

        [Fact]
        public void BookingMadeEventMustContainContactDetails()
        {
            EventMustContainContactDetails<BookingMade>();
        }

        [Fact]
        public void BookingCancelledEventMustContainContactDetails()
        {
            EventMustContainContactDetails<BookingCancelled>();
        }

        private void EventMustContainContactDetails<T>() where T : PatientBookingNotificationEventBase, new()
        {
            var booking = BuildBooking(false);
            Assert.Throws<ArgumentException>(() => _sut.BuildBookingEvents<T>(booking)).ParamName.Should().Be(nameof(booking.ContactDetails));
        }

        private void BuildsEventCorrectly<T>() where T : PatientBookingNotificationEventBase, new()
        {
            var booking = BuildBooking();
            var events = _sut.BuildBookingEvents<T>(booking);

            events.Any(e => e.NotificationType == NotificationType.Email).Should().BeTrue();
            events.Any(e => e.NotificationType == NotificationType.Sms).Should().BeTrue();

            foreach (var e in events)
            {
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
        }

        private static Booking BuildBooking(bool withContact = true)
        {
            ContactItem[] contact = withContact ? [ new ContactItem { Type = ContactItemType.Email, Value = "test@tempuri.org"}, new ContactItem { Type = ContactItemType.Phone, Value = "1234567890"}] : null;
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
