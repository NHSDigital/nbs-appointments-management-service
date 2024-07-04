using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Flurl;
using Gherkin.Ast;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.SiteConfiguration;

[FeatureFile("./Scenarios/SiteConfiguration/SiteConfiguration.feature")]
public class SiteConfigurationFeatureSteps : BaseFeatureSteps
{
    private string SiteConfigurationUrl => Url.Combine(AppointmentsApiUrl, "site-configuration").SetQueryParam("site", GetSiteId());
    private string ScheduleTemplateUrl => Url.Combine(AppointmentsApiUrl, "templates").SetQueryParam("site", GetSiteId());

    [When(@"service type '(.+)' is disabled")]
    public async Task ServiceTypeIsDisabled(string serviceTypeToDisable)
    {
        var siteId = GetSiteId();
        var cosmosDoc = await Client.GetContainer("appts", "index_data").ReadItemAsync<Core.SiteConfiguration>(siteId, new Microsoft.Azure.Cosmos.PartitionKey("site_configuration"));
        var currentSiteConfiguration = cosmosDoc.Resource;

        var newSiteConfiguration = new Core.SiteConfiguration()
        {
            Site = currentSiteConfiguration.Site,
            ServiceConfiguration = currentSiteConfiguration.ServiceConfiguration
                .Select(x => x.Code == serviceTypeToDisable ? x with { Enabled = false } : x).ToList()
        };
        var newSiteConfigurationJson = JsonResponseWriter.Serialize(newSiteConfiguration);
        await Http.PostAsync(SiteConfigurationUrl, new StringContent(newSiteConfigurationJson, Encoding.UTF8, "application/json"));
    }

    [Then("the following week templates exist")]
    public async Task TheFollowingScheduleTemplatesExist(Gherkin.Ast.DataTable expectedScheduleTemplates)
    {
        var scheduleTemplateStream = await Http.GetStreamAsync(ScheduleTemplateUrl);
        var scheduleTemplate = await JsonRequestReader.ReadRequestAsync<GetTemplateResponse>(scheduleTemplateStream);
        var expected = new ExpectedScheduleTemplateTable(expectedScheduleTemplates);
        
        scheduleTemplate.Templates.Length.Should().Be(expected.NumberOfSchedules);
        var schedule = scheduleTemplate.Templates.First();
        schedule.Items.First().ScheduleBlocks.Length.Should().Be(expected.NumberOfScheduleBlocks);
        schedule.Items.First().Days.Should().Equal(expected.Days);
        var scheduleBlock = scheduleTemplate.Templates.First().Items.First().ScheduleBlocks.First();
        scheduleBlock.Services.Should().BeEquivalentTo(expected.Services);
        scheduleBlock.From.Should().Be(expected.From);
        scheduleBlock.Until.Should().Be(expected.Until);
    }

    private class ExpectedScheduleTemplateTable
    {
        private readonly DataTable _dataTable;
    
        public ExpectedScheduleTemplateTable(DataTable dataTable)
        {
            _dataTable = dataTable;
        }

        public int NumberOfSchedules => _dataTable.Rows.Count() - 1;
        
        public int NumberOfScheduleBlocks => _dataTable.Rows.Count() - 1;

        public TimeOnly From => TimeOnly.ParseExact(_dataTable.Rows.ElementAt(1).Cells.ElementAt(1).Value, "HH:mm");

        public TimeOnly Until => TimeOnly.ParseExact(_dataTable.Rows.ElementAt(1).Cells.ElementAt(2).Value, "HH:mm");
        
        public string[] Services => _dataTable.Rows.ElementAt(1).Cells.ElementAt(3).Value.Split(",", StringSplitOptions.TrimEntries);
        
        public DayOfWeek[] Days
        {
            get
            {
                var days = _dataTable
                    .Rows.ElementAt(1)
                    .Cells.ElementAt(0)
                    .Value.Split(",", StringSplitOptions.TrimEntries);
                return days.Select(x =>
                {
                    Enum.TryParse(x, out DayOfWeek parsed);
                    return parsed;
                }).ToArray();
            }  
        }
    }
}