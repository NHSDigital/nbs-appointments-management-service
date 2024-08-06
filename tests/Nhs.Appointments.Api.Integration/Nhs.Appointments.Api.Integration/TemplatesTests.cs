using Nhs.Appointments.ApiClient.Models;

namespace Nhs.Appointments.Api.Integration
{
    public class TemplatesTests : IClassFixture<AzureFunctionsTestcontainersFixture>
    {
        private readonly AzureFunctionsTestcontainersFixture _azureFunctionsTestcontainersFixture;
        private const string KnownSiteId = "1";
        public TemplatesTests(AzureFunctionsTestcontainersFixture azureFunctionsTestcontainersFixture)
        {
            _azureFunctionsTestcontainersFixture = azureFunctionsTestcontainersFixture;
        }

        [Fact]
        public async void CanGetTemplateForSite()
        {
            var template = await _azureFunctionsTestcontainersFixture.ApiClient.Templates.GetTemplate(KnownSiteId);
            Assert.NotNull(template);
        }

        [Fact]
        public async void CanGetTemplateAssignments()
        {
            var assignments = await _azureFunctionsTestcontainersFixture.ApiClient.Templates.GetTemplateAssignments(KnownSiteId);
            Assert.NotNull(assignments);
        }

        [Fact]
        public async void CanSetTemplate()
        {
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

            var result = await _azureFunctionsTestcontainersFixture.ApiClient.Templates.SetTemplate(weekTemplate);
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async void CanSetTemplateAssignment()
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
                         Days = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday]
                    }
                ],
                Id = "INT1"
            };

            await _azureFunctionsTestcontainersFixture.ApiClient.Templates.SetTemplate(weekTemplate);

            await _azureFunctionsTestcontainersFixture.ApiClient.Templates.SetTemplateAssignment(KnownSiteId, templateAssignments);
        }
    }
}
