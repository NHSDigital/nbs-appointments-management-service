using Nhs.Appointments.ApiClient.Models;

namespace Nhs.Appointments.Api.Integration
{
    public class BookingsTests : IClassFixture<AzureFunctionsTestcontainersFixture>
    {
        private readonly AzureFunctionsTestcontainersFixture _azureFunctionsTestcontainersFixture;
        private const string KnownSiteId = "1";
        public BookingsTests(AzureFunctionsTestcontainersFixture azureFunctionsTestcontainersFixture)
        {
            _azureFunctionsTestcontainersFixture = azureFunctionsTestcontainersFixture;
        }

        [Fact]
        public async void CanQueryAvailability()
        {
            var slots = await _azureFunctionsTestcontainersFixture.ApiClient.Bookings.QueryAvailability([KnownSiteId], "COVID:12_15", DateOnly.FromDateTime(DateTime.Now.AddYears(-1)), DateOnly.FromDateTime(DateTime.Now.AddYears(1)), QueryType.Days);
            Assert.NotNull(slots);
        }

        [Fact]
        public async void CanMakeBooking()
        {
            var bookingReference = await MakeBooking();
            await _azureFunctionsTestcontainersFixture.ApiClient.Bookings.CancelBooking(bookingReference, KnownSiteId);
            Assert.True(bookingReference.Length > 0);
        }

        [Fact]
        public async void CanCancelBooking()
        {
            var bookingReference = await MakeBooking();
            var result = await _azureFunctionsTestcontainersFixture.ApiClient.Bookings.CancelBooking(bookingReference, KnownSiteId);
            Assert.Equal("cancelled", result.Status);
            Assert.Equal(bookingReference, result.BookingReference);
        }

        private async Task<string> MakeBooking()
        {
            TemplateAssignment[] templateAssignments =
            [
                new TemplateAssignment("2024-01-01", "2054-01-01", "INT1")
            ];

            var weekTemplate = new ApiClient.Models.WeekTemplate
            {
                Site = KnownSiteId,
                Name = "Integration Test Template",
                Items =
                [
                    new Schedule
                    {
                         Days = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday],
                         ScheduleBlocks = [new ScheduleBlock { From = new TimeOnly(9, 0), Until = new TimeOnly(17, 30), Services = ["COVID:12_15"] } ]
                    }
                ],
                Id = "INT1"
            };

            await _azureFunctionsTestcontainersFixture.ApiClient.Templates.SetTemplate(weekTemplate);
            await _azureFunctionsTestcontainersFixture.ApiClient.Templates.SetTemplateAssignment(KnownSiteId, templateAssignments);


            await _azureFunctionsTestcontainersFixture.ApiClient.Sites.SetSiteConfiguration(new Appointments.ApiClient.Models.SiteConfiguration
            {
                InformationForCitizen = "Some information",
                ReferenceNumberGroup = 14,
                Site = KnownSiteId,
                ServiceConfiguration = [new ServiceConfiguration("COVID:12_15", "Covid Vaccination", 10, true)]
            });

            var result = await _azureFunctionsTestcontainersFixture.ApiClient.Bookings.MakeBooking(KnownSiteId, DateTime.Now.AddDays(1), "COVID:12_15", "default", new Appointments.ApiClient.Models.AttendeeDetails("1234678890", "David", "Testpatient", "1980-01-01"));
            return result.BookingReference;
        }
    }
}
