// namespace Nhs.Appointments.Core.UnitTests;
//
// public class AvailabilityCalculatorTests
// {
//     private readonly AvailabilityCalculator _sut;
//     private readonly Mock<IBookingsDocumentStore> _bookingDocumentStore = new();
//     private readonly Mock<IAvailabilityStore> _availabilityDocumentStore = new();
//     private readonly Mock<TimeProvider> _timeProvider = new();
//
//     public AvailabilityCalculatorTests() => _sut = new AvailabilityCalculator(_availabilityDocumentStore.Object,
//         _bookingDocumentStore.Object, _timeProvider.Object);
//
//     [Fact]
//     public async Task CalculateAvailability_ReturnsEmpty_WhenSessionsAndServiceDoNotMatch()
//     {
//         var sessions = new[]
//         {
//             CreateSessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,10,0,0), "COVID")
//         };
//         _availabilityDocumentStore.Setup(x => x.GetSessions("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).ReturnsAsync(sessions);
//         var results = await _sut.CalculateAvailability("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", "FLU", new DateOnly(2077, 1, 1), new DateOnly(2077, 1, 2));
//         results.Should().BeEmpty();
//     }
//
//     [Fact]
//     public async Task CalculateAvailability_ReturnsSlots_ByDividingSessions()
//     {
//         var sessions = new[]
//         {
//             CreateSessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,10,0,0), 15, 1, "COVID")
//         };
//         _availabilityDocumentStore.Setup(x => x.GetSessions("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).ReturnsAsync(sessions);
//         var results = await _sut.CalculateAvailability("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", "COVID", new DateOnly(2077, 1, 1), new DateOnly(2077, 1, 2));
//
//         var expectedResults = new[]
//         {
//             new SessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,9,15,0)){ Services = new List<string> {"COVID"}, Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,15,0), new DateTime(2077,1,1,9,30,0)){ Services = new List<string> {"COVID"}, Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,30,0), new DateTime(2077,1,1,9,45,0)){ Services = new List<string> {"COVID"}, Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,45,0), new DateTime(2077,1,1,10,0,0)){ Services = new List<string> {"COVID"}, Capacity = 1 },
//         };
//
//         results.Should().BeEquivalentTo(expectedResults);
//     }
//
//     [Fact]
//     public async Task CalculateAvailability_AllowsOverlappingSessions()
//     {
//         var sessions = new[]
//         {
//             CreateSessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,10,0,0), 15, 1, "COVID"),
//             CreateSessionInstance(new DateTime(2077,1,1,9,30,0), new DateTime(2077,1,1,10,30,0), 15, 1, "COVID")
//         };
//         _availabilityDocumentStore.Setup(x => x.GetSessions("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).ReturnsAsync(sessions);
//         var results = await _sut.CalculateAvailability("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", "COVID", new DateOnly(2077, 1, 1), new DateOnly(2077, 1, 2));
//
//         var expectedResults = new[]
//         {
//             new SessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,9,15,0)){ Services = new []{"COVID"}, Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,15,0), new DateTime(2077,1,1,9,30,0)){ Services = new []{"COVID"}, Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,30,0), new DateTime(2077,1,1,9,45,0)){ Services = new []{"COVID"}, Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,45,0), new DateTime(2077,1,1,10,0,0)){ Services = new []{"COVID"}, Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,30,0), new DateTime(2077,1,1,9,45,0)){ Services = new []{"COVID"}, Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,45,0), new DateTime(2077,1,1,10,0,0)){ Services = new []{"COVID"}, Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,10,0,0), new DateTime(2077,1,1,10,15,0)){ Services = new []{"COVID"}, Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,10,15,0), new DateTime(2077,1,1,10,30,0)){ Services = new []{"COVID"}, Capacity = 1 },
//         };
//
//         results.Should().BeEquivalentTo(expectedResults);
//     }
//
//     [Fact]
//     public async Task CalculateAvailability_FiltersOutSessions_WithoutMatchingService()
//     {
//         var sessions = new[]
//         {
//             CreateSessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,10,0,0), 15, 1, "COVID"),
//             CreateSessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,10,0,0), 15, 1, "FLU")
//         };
//         _availabilityDocumentStore.Setup(x => x.GetSessions("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).ReturnsAsync(sessions);
//         var results = await _sut.CalculateAvailability("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", "COVID", new DateOnly(2077, 1, 1), new DateOnly(2077, 1, 2));
//
//         var expectedResults = new[]
//         {
//             new SessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,9,15,0)){ Services = new []{"COVID"}, Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,15,0), new DateTime(2077,1,1,9,30,0)){ Services = new []{"COVID"}, Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,30,0), new DateTime(2077,1,1,9,45,0)){ Services = new []{"COVID"}, Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,45,0), new DateTime(2077,1,1,10,0,0)){ Services = new []{"COVID"}, Capacity = 1 },
//         };
//
//         results.Should().BeEquivalentTo(expectedResults);
//     }
//
//     [Fact]
//     public async Task CalculateAvailability_AsjustsSlotCapacity_BasedOnBookings()
//     {
//         var sessions = new[]
//         {
//             CreateSessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,10,0,0), 15, 2, "COVID"),
//         };
//
//         var bookings = new[]
//         {
//             CreateTestBooking(new DateTime(2077, 1, 1, 9, 0, 0), 15, "COVID", "2de5bb57-060f-4cb5-b14d-16587d0c2e8f")
//         };
//
//         _availabilityDocumentStore.Setup(x => x.GetSessions("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).ReturnsAsync(sessions);
//         _bookingDocumentStore.Setup(x => x.GetInDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), "2de5bb57-060f-4cb5-b14d-16587d0c2e8f")).ReturnsAsync(bookings);
//
//         var results = await _sut.CalculateAvailability("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", "COVID", new DateOnly(2077, 1, 1), new DateOnly(2077, 1, 2));
//
//         var expectedResults = new[]
//         {
//             new SessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,9,15,0)){ Services = new []{"COVID"}, Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,15,0), new DateTime(2077,1,1,9,30,0)){ Services = new []{"COVID"}, Capacity = 2 },
//             new SessionInstance(new DateTime(2077,1,1,9,30,0), new DateTime(2077,1,1,9,45,0)){ Services = new []{"COVID"}, Capacity = 2 },
//             new SessionInstance(new DateTime(2077,1,1,9,45,0), new DateTime(2077,1,1,10,0,0)){ Services = new []{"COVID"}, Capacity = 2 },
//         };
//
//         results.Should().BeEquivalentTo(expectedResults);
//     }
//
//     [Fact]
//     public async Task CalculateAvailability_AsjustsSlotCapacity_BasedOnBookings_IgnoresCancelledAppointments()
//     {
//         var sessions = new[]
//         {
//             CreateSessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,10,0,0), 15, 2, "COVID"),
//         };
//
//         var bookings = new[]
//         {
//             CreateTestBooking(new DateTime(2077, 1, 1, 9, 0, 0), 15, "COVID", "2de5bb57-060f-4cb5-b14d-16587d0c2e8f", AppointmentStatus.Cancelled)
//         };
//
//         _availabilityDocumentStore.Setup(x => x.GetSessions("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).ReturnsAsync(sessions);
//         _bookingDocumentStore.Setup(x => x.GetInDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), "2de5bb57-060f-4cb5-b14d-16587d0c2e8f")).ReturnsAsync(bookings);
//
//         var results = await _sut.CalculateAvailability("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", "COVID", new DateOnly(2077, 1, 1), new DateOnly(2077, 1, 2));
//
//         var expectedResults = new[]
//         {
//             new SessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,9,15,0)){ Services = new []{"COVID"}, Capacity = 2 },
//             new SessionInstance(new DateTime(2077,1,1,9,15,0), new DateTime(2077,1,1,9,30,0)){ Services = new []{"COVID"}, Capacity = 2 },
//             new SessionInstance(new DateTime(2077,1,1,9,30,0), new DateTime(2077,1,1,9,45,0)){ Services = new []{"COVID"}, Capacity = 2 },
//             new SessionInstance(new DateTime(2077,1,1,9,45,0), new DateTime(2077,1,1,10,0,0)){ Services = new []{"COVID"}, Capacity = 2 },
//         };
//
//         results.Should().BeEquivalentTo(expectedResults);
//     }
//
//     [Fact]
//     public async Task CalculateAvailability_AsjustsSlotCapacity_BasedOnBookings_IgnoresExpiredProvisionalAppointments()
//     {
//         _timeProvider.Setup(t => t.GetUtcNow()).Returns(DateTime.UtcNow);
//
//         var sessions = new[]
//         {
//             CreateSessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,10,0,0), 15, 2, "COVID"),
//         };
//
//         var bookings = new[]
//         {
//             CreateTestBooking(new DateTime(2077, 1, 1, 9, 0, 0), 15, "COVID", "2de5bb57-060f-4cb5-b14d-16587d0c2e8f", AppointmentStatus.Provisional, DateTime.Now.AddMinutes(-6))
//         };
//
//         _availabilityDocumentStore.Setup(x => x.GetSessions("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).ReturnsAsync(sessions);
//         _bookingDocumentStore.Setup(x => x.GetInDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), "2de5bb57-060f-4cb5-b14d-16587d0c2e8f")).ReturnsAsync(bookings);
//
//         var results = await _sut.CalculateAvailability("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", "COVID", new DateOnly(2077, 1, 1), new DateOnly(2077, 1, 2));
//
//         var expectedResults = new[]
//         {
//             new SessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,9,15,0)){ Services = new []{"COVID"}, Capacity = 2 },
//             new SessionInstance(new DateTime(2077,1,1,9,15,0), new DateTime(2077,1,1,9,30,0)){ Services = new []{"COVID"}, Capacity = 2 },
//             new SessionInstance(new DateTime(2077,1,1,9,30,0), new DateTime(2077,1,1,9,45,0)){ Services = new []{"COVID"}, Capacity = 2 },
//             new SessionInstance(new DateTime(2077,1,1,9,45,0), new DateTime(2077,1,1,10,0,0)){ Services = new []{"COVID"}, Capacity = 2 },
//         };
//
//         results.Should().BeEquivalentTo(expectedResults);
//     }
//
//     [Fact]
//     public async Task CalculateAvailability_FiltersSlots_WithNoRemainingCapacity()
//     {
//         var sessions = new[]
//         {
//             CreateSessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,10,0,0), 15, 2, "COVID"),
//         };
//
//         var bookings = new[]
//         {
//             CreateTestBooking(new DateTime(2077, 1, 1, 9, 0, 0), 15, "COVID", "2de5bb57-060f-4cb5-b14d-16587d0c2e8f"),
//             CreateTestBooking(new DateTime(2077, 1, 1, 9, 0, 0), 15, "COVID", "2de5bb57-060f-4cb5-b14d-16587d0c2e8f")
//         };
//
//         _availabilityDocumentStore.Setup(x => x.GetSessions("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).ReturnsAsync(sessions);
//         _bookingDocumentStore.Setup(x => x.GetInDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), "2de5bb57-060f-4cb5-b14d-16587d0c2e8f")).ReturnsAsync(bookings);
//
//         var results = await _sut.CalculateAvailability("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", "COVID", new DateOnly(2077, 1, 1), new DateOnly(2077, 1, 2));
//
//         var expectedResults = new[]
//         {            
//             new SessionInstance(new DateTime(2077,1,1,9,15,0), new DateTime(2077,1,1,9,30,0)){ Services = new []{"COVID"}, Capacity = 2 },
//             new SessionInstance(new DateTime(2077,1,1,9,30,0), new DateTime(2077,1,1,9,45,0)){ Services = new []{"COVID"}, Capacity = 2 },
//             new SessionInstance(new DateTime(2077,1,1,9,45,0), new DateTime(2077,1,1,10,0,0)){ Services = new []{"COVID"}, Capacity = 2 },
//         };
//
//         results.Should().BeEquivalentTo(expectedResults);
//     }
//
//     [Fact]
//     public async Task CalculateAvailability_MatchesBookings_ToCorrectSession()
//     {
//         var sessions = new[]
//         {
//             CreateSessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,10,0,0), 15, 2, "COVID"),
//             CreateSessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,10,0,0), 30, 2, "COVID"),
//         };
//
//         var bookings = new[]
//         {
//             CreateTestBooking(new DateTime(2077, 1, 1, 9, 0, 0), 15, "COVID", "2de5bb57-060f-4cb5-b14d-16587d0c2e8f"),
//         };
//
//         _availabilityDocumentStore.Setup(x => x.GetSessions("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).ReturnsAsync(sessions);
//         _bookingDocumentStore.Setup(x => x.GetInDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), "2de5bb57-060f-4cb5-b14d-16587d0c2e8f")).ReturnsAsync(bookings);
//
//         var results = await _sut.CalculateAvailability("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", "COVID", new DateOnly(2077, 1, 1), new DateOnly(2077, 1, 2));
//
//         var expectedResults = new[]
//         {
//             new SessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,9,15,0)){ Services = new []{"COVID"}, Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,15,0), new DateTime(2077,1,1,9,30,0)){ Services = new []{"COVID"}, Capacity = 2 },
//             new SessionInstance(new DateTime(2077,1,1,9,30,0), new DateTime(2077,1,1,9,45,0)){ Services = new []{"COVID"}, Capacity = 2 },
//             new SessionInstance(new DateTime(2077,1,1,9,45,0), new DateTime(2077,1,1,10,0,0)){ Services = new []{"COVID"}, Capacity = 2 },
//             new SessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,9,30,0)){ Services = new []{"COVID"}, Capacity = 2 },
//             new SessionInstance(new DateTime(2077,1,1,9,30,0), new DateTime(2077,1,1,10,0,0)){ Services = new []{"COVID"}, Capacity = 2 },
//         };
//
//         results.Should().BeEquivalentTo(expectedResults);
//     }
//
//     [Fact]
//     public async Task CalculateAvailability_ReturnsAllSessions()
//     {
//         var sessions = new[]
//         {
//             CreateSessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,10,0,0), 15, 1, "COVID"),
//             CreateSessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,10,0,0), 15, 1, "FLU"),
//             CreateSessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,10,0,0), 15, 1, "RSV")
//         };
//         _availabilityDocumentStore.Setup(x => x.GetSessions("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).ReturnsAsync(sessions);
//         var results = await _sut.CalculateAvailability("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", "*", new DateOnly(2077, 1, 1), new DateOnly(2077, 1, 2));
//
//         var expectedResults = new[]
//         {
//             new SessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,9,15,0)){ Services = ["COVID"], Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,15,0), new DateTime(2077,1,1,9,30,0)){ Services = ["COVID"], Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,30,0), new DateTime(2077,1,1,9,45,0)){ Services = ["COVID"], Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,45,0), new DateTime(2077,1,1,10,0,0)){ Services = ["COVID"], Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,9,15,0)){ Services = ["FLU"], Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,15,0), new DateTime(2077,1,1,9,30,0)){ Services = ["FLU"], Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,30,0), new DateTime(2077,1,1,9,45,0)){ Services = ["FLU"], Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,45,0), new DateTime(2077,1,1,10,0,0)){ Services = ["FLU"], Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,9,15,0)){ Services = ["RSV"], Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,15,0), new DateTime(2077,1,1,9,30,0)){ Services = ["RSV"], Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,30,0), new DateTime(2077,1,1,9,45,0)){ Services = ["RSV"], Capacity = 1 },
//             new SessionInstance(new DateTime(2077,1,1,9,45,0), new DateTime(2077,1,1,10,0,0)){ Services = ["RSV"], Capacity = 1 },
//         };
//
//         results.Should().BeEquivalentTo(expectedResults);
//     }
//
//     private static SessionInstance CreateSessionInstance(DateTime from, DateTime until, params string[] services)
//     {
//         return CreateSessionInstance(from, until, 5, 1, services);
//     }
//
//     private static SessionInstance CreateSessionInstance(DateTime from, DateTime until, int slotLength, int capacity, params string[] services)
//     {
//         var sessionInstance = new SessionInstance(from, until);
//         sessionInstance.Services = services;
//         sessionInstance.Capacity = capacity;
//         sessionInstance.SlotLength = slotLength;
//         return sessionInstance;
//     }
//     
//     private static Booking CreateTestBooking(DateTime appointmentDateAndTime, int appointmentDuration, string service, string site, AppointmentStatus status = AppointmentStatus.Booked, DateTime? created = null)
//     {
//         created = created ?? DateTime.Now;
//         var testBooking = new Booking
//         {
//             Reference = "123",
//             From = appointmentDateAndTime,
//             Duration = appointmentDuration,
//             Service = service,
//             Site = site,            
//             Status = status,
//             AttendeeDetails = new AttendeeDetails()
//             {
//                 NhsNumber = "999999999",
//                 DateOfBirth = new DateOnly(2000, 01, 01),
//                 FirstName = "FirstName",
//                 LastName = "LastName"
//             },
//             Created = created.Value
//         };
//         return testBooking;
//     }
// }
