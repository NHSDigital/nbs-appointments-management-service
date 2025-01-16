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
            var expectedNotifications = new T[]
            {
                new()
                {
                    FirstName = "firstname",
                    LastName = "lastname",
                    From = new DateTime(2024, 11, 14),
                    Reference = "reference",
                    Service = "service",
                    Site = "site",
                    NotificationType = NotificationType.Email,
                    Destination = "test@tempuri.org"
                },
                new()
                {
                    FirstName = "firstname",
                    LastName = "lastname",
                    From = new DateTime(2024, 11, 14),
                    Reference = "reference",
                    Service = "service",
                    Site = "site",
                    NotificationType = NotificationType.Sms,
                    Destination = "1234567890"
                }
            };
            var booking = BuildBooking();
            var events = _sut.BuildBookingEvents<T>(booking);

            events.Should().BeEquivalentTo(expectedNotifications);
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
