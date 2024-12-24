using FluentAssertions;
using System.Text;
using Nhs.Appointments.Persistance.Models;

namespace CsvDataTool.UnitTests
{
    public class SiteCsvDataReaderTests
    {
        [Fact]
        public async Task  CanReadSiteData()
        {         
            string[] inputRows =
                [
                "site1,\"test site 1\",\"123 test street\",\"01234 567890\",\"1.0\",\"60.0\",\"test icb1\",\"Yorkshire\",,true,True,False,false,\"YES\",no,true,true,NO",
                "site2,\"test site 2\",\"123 test street\",\"01234 567890\",1.0,60.0,\"test icb2\",\"Yorkshire\",,true,True,False,false,\"YES\",no,true,true,NO",
                "site3,\"test site 3\",\"123 test street\",\"01234 567890\",1.0,60.0,\"test icb3\",\"Yorkshire\",,true,,False,\"oranges\",\"YES\",no,true,true,NO",
                ];          

            var input = BuildInputCsv(inputRows);
            var sites = new List<SiteDocument>();

            var sut = new SiteCsvReader(new StringReader(input));
            var report = await sut.ReadAndProcessAsync(
                s =>
                {
                    sites.Add(s);
                    return Task.CompletedTask;
                });
            sites.Should().NotBeNull();
            sites.Count.Should().Be(3);
            sites[1].Name.Should().Be("test site 2");
            sites[1].Id.Should().Be("site2");
            sites[1].Address.Should().Be("123 test street");
            sites[1].PhoneNumber.Should().Be("01234 567890");
            sites[1].Location.Coordinates[0].Should().Be(1.0);
            sites[1].Location.Coordinates[1].Should().Be(60.0);
            sites[1].Location.Type.Should().Be("Point");
            sites[1].IntegratedCareBoard.Should().Be("test icb2");
            sites[1].Region.Should().Be("Yorkshire");
            sites[1].AttributeValues.First(x => x.Id == "accessibility/accessible_toilet").Value.Should().Be("True");
            sites[1].AttributeValues.First(x => x.Id == "accessibility/braille_translation_service").Value.Should().Be("True");
            sites[1].AttributeValues.First(x => x.Id == "accessibility/disabled_car_parking").Value.Should().Be("False");
            sites[1].AttributeValues.First(x => x.Id == "accessibility/car_parking").Value.Should().Be("False");
            sites[1].AttributeValues.First(x => x.Id == "accessibility/induction_loop").Value.Should().Be("True");
            sites[1].AttributeValues.First(x => x.Id == "accessibility/sign_language_service").Value.Should().Be("False");
            sites[1].AttributeValues.First(x => x.Id == "accessibility/step_free_access").Value.Should().Be("True");
            sites[1].AttributeValues.First(x => x.Id == "accessibility/text_relay").Value.Should().Be("True");
            sites[1].AttributeValues.First(x => x.Id == "accessibility/wheelchair_access").Value.Should().Be("False");

            report.Count().Should().Be(3);
            report.Any(r => !r.Success).Should().BeFalse();
        }

        private string BuildInputCsv(IEnumerable<string> dataLines)
        {
            var result = new StringBuilder(Header);
            result.Append("\r\n");
            foreach (var line in dataLines)
            {
                result.Append($"{line}\r\n");
            }

            return result.ToString();
        }

        private const string Header = "ID,Name,Address,PhoneNumber,Longitude,Latitude,ICB,Region,Site type,accessible_toilet,braille_translation_service,disabled_car_parking,car_parking,induction_loop,sign_language_service,step_free_access,text_relay,wheelchair_access";
    }
}
