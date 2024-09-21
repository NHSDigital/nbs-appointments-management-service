using Nhs.Appointments.ApiClient.Models;

namespace Nhs.Appointments.Api.Client.Integration
{
    public class TemplatesClientTests : IntegrationTestBase
    {
        [Fact (Skip = "All tests in the Api.Client.Integration project are WIP and are not expected to pass.")]
        public async void CanGetTemplateForSite()
        {
            var template = await ApiClient.Templates.GetTemplate(KnownSiteId);
            Assert.NotNull(template);
        }

        [Fact (Skip = "All tests in the Api.Client.Integration project are WIP and are not expected to pass.")]
        public async void CanGetTemplateAssignments()
        {
            var assignments = await ApiClient.Templates.GetTemplateAssignments(KnownSiteId);
            Assert.NotNull(assignments);
        }

        [Fact (Skip = "All tests in the Api.Client.Integration project are WIP and are not expected to pass.")]
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

            var result = await ApiClient.Templates.SetTemplate(weekTemplate);
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact (Skip = "All tests in the Api.Client.Integration project are WIP and are not expected to pass.")]
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

            await ApiClient.Templates.SetTemplate(weekTemplate);

            await ApiClient.Templates.SetTemplateAssignment(KnownSiteId, templateAssignments);
        }
    }
}