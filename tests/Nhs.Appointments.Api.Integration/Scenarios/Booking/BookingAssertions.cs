using FluentAssertions;
using Nhs.Appointments.Persistance.Models;
using System.Collections.Generic;
using System.Linq;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking
{
    public static class BookingAssertions
    {
        public static void BookingsAreEquivalent(BookingDocument actual, BookingDocument expected)
        {
            expected.ContactDetails.Should().BeEquivalentTo(actual.ContactDetails);
            expected.Duration.Should().Be(actual.Duration);
            expected.AttendeeDetails.Should().BeEquivalentTo(actual.AttendeeDetails);
            expected.Status.Should().Be(actual.Status);
            expected.Created.Year.Should().Be(actual.Created.Year);
            expected.Created.Month.Should().Be(actual.Created.Month);
            expected.Created.Day.Should().Be(actual.Created.Day);
            expected.DocumentType.Should().Be(actual.DocumentType);
            expected.From.Should().Be(actual.From);
            expected.Reference.Should().Be(actual.Reference);
            expected.Service.Should().Be(actual.Service);
            expected.Site.Should().Be(actual.Site);
        }

        public static void BookingsAreEquivalent(Core.Booking actual, Core.Booking expected)
        {
            expected.ContactDetails.Should().BeEquivalentTo(actual.ContactDetails);
            expected.Duration.Should().Be(actual.Duration);
            expected.AttendeeDetails.Should().BeEquivalentTo(actual.AttendeeDetails);
            expected.Status.Should().Be(actual.Status);
            expected.Created.Year.Should().Be(actual.Created.Year);
            expected.Created.Month.Should().Be(actual.Created.Month);
            expected.Created.Day.Should().Be(actual.Created.Day);
            expected.From.Should().Be(actual.From);
            expected.Reference.Should().Be(actual.Reference);
            expected.Service.Should().Be(actual.Service);
            expected.Site.Should().Be(actual.Site);
        }

        public static void BookingsAreEquivalent(IEnumerable<Core.Booking> actual, IEnumerable<Core.Booking> expected)
        {
            foreach (var expectedItem in expected)
            {
                var actualItem = actual.Single(b => b.Reference == expectedItem.Reference);
                BookingsAreEquivalent(expectedItem, actualItem);
            }
        }

    }
}
