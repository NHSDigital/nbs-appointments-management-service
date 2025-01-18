using System.Text;
using FluentAssertions;
using Moq;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace CsvDataTool.UnitTests;

public class CsvProcessorTests
{
    private const string SitesHeader =
        "OdsCode,Name,Address,PhoneNumber,Longitude,Latitude,ICB,Region,Site type,accessible_toilet,braille_translation_service,disabled_car_parking,car_parking,induction_loop,sign_language_service,step_free_access,text_relay,wheelchair_access";

    private const string UsersHeader = "User,Site";
    private const string ApiUserHeader = "ClientId,ApiSigningKey";

    [Fact]
    public async Task CanReadUserData()
    {
        string[] inputRows =
        [
            "test1@nhs.net,ABC01",
            "test1@nhs.net,ABC02",
            "test2@nhs.net,ABC01",
            "test2@nhs.net,ABC03",
        ];

        var expectedUserDocuments = new UserDocument[]
        {
            new()
            {
                Id = "test1@nhs.net",
                DocumentType = "user",
                LatestAcceptedEulaVersion = DateOnly.MinValue,
                RoleAssignments =
                [
                    new() { Role = "canned:user-manager", Scope = "site:ABC01" },
                    new() { Role = "canned:site-details-manager", Scope = "site:ABC01" },
                    new() { Role = "canned:availability-manager", Scope = "site:ABC01" },
                    new() { Role = "canned:appointment-manager", Scope = "site:ABC01" },
                    new() { Role = "canned:user-manager", Scope = "site:ABC02" },
                    new() { Role = "canned:site-details-manager", Scope = "site:ABC02" },
                    new() { Role = "canned:availability-manager", Scope = "site:ABC02" },
                    new() { Role = "canned:appointment-manager", Scope = "site:ABC02" }
                ]
            },
            new()
            {
                Id = "test2@nhs.net",
                DocumentType = "user",
                LatestAcceptedEulaVersion = DateOnly.MinValue,
                RoleAssignments =
                [
                    new() { Role = "canned:user-manager", Scope = "site:ABC01" },
                    new() { Role = "canned:site-details-manager", Scope = "site:ABC01" },
                    new() { Role = "canned:availability-manager", Scope = "site:ABC01" },
                    new() { Role = "canned:appointment-manager", Scope = "site:ABC01" },
                    new() { Role = "canned:user-manager", Scope = "site:ABC03" },
                    new() { Role = "canned:site-details-manager", Scope = "site:ABC03" },
                    new() { Role = "canned:availability-manager", Scope = "site:ABC03" },
                    new() { Role = "canned:appointment-manager", Scope = "site:ABC03" }
                ]
            }
        };

        var input = BuildInputCsv(UsersHeader, inputRows);
        var actualUserDocuments = new List<UserDocument>();
        var mockFileOperations = new Mock<IFileOperations>();
        mockFileOperations.Setup(x => x.OpenText(It.IsAny<FileInfo>())).Returns(new StringReader(input));
        mockFileOperations.Setup(x => x.WriteDocument<UserDocument>(It.IsAny<UserDocument>(), It.IsAny<string>()))
            .Callback<UserDocument, string>((doc, path) => actualUserDocuments.Add(doc));

        var sut = new UserDataImportHandler(mockFileOperations.Object);
        var report = await sut.ProcessFile(new FileInfo("test.csv"), new DirectoryInfo("/out"));

        actualUserDocuments.Should().BeEquivalentTo(expectedUserDocuments);

        report.Count().Should().Be(4);
        report.Any(r => !r.Success).Should().BeFalse();
    }

    [Fact]
    public async Task CanReadApiUserData()
    {
        string[] inputRows =
        [
            "test1,ABC123",
            "test2,DEF345"
        ];

        var expectedUserDocuments = new UserDocument[]
        {
            new()
            {
                Id = "api@test1",
                DocumentType = "user",
                ApiSigningKey = "ABC123",
                LatestAcceptedEulaVersion = DateOnly.MinValue,
                RoleAssignments =
                [
                    new() { Role = "system:api-user", Scope = "global" }
                ]
            },
            new()
            {
                Id = "api@test2",
                DocumentType = "user",
                ApiSigningKey = "DEF345",
                LatestAcceptedEulaVersion = DateOnly.MinValue,
                RoleAssignments =
                [
                    new() { Role = "system:api-user", Scope = "global" }
                ]
            }
        };

        var input = BuildInputCsv(ApiUserHeader, inputRows);
        var actualUserDocuments = new List<UserDocument>();
        var mockFileOperations = new Mock<IFileOperations>();
        mockFileOperations.Setup(x => x.OpenText(It.IsAny<FileInfo>())).Returns(new StringReader(input));
        mockFileOperations.Setup(x => x.WriteDocument<UserDocument>(It.IsAny<UserDocument>(), It.IsAny<string>()))
            .Callback<UserDocument, string>((doc, path) => actualUserDocuments.Add(doc));

        var sut = new ApiUserDataImportHandler(mockFileOperations.Object);
        var report = await sut.ProcessFile(new FileInfo("test.csv"), new DirectoryInfo("/out"));

        actualUserDocuments.Should().BeEquivalentTo(expectedUserDocuments);

        report.Count().Should().Be(2);
        report.Any(r => !r.Success).Should().BeFalse();
    }

    [Fact]
    public async Task CanReadSiteData()
    {
        string[] inputRows =
        [
            "site1,\"test site 1\",\"123 test street\",\"01234 567890\",\"1.0\",\"60.0\",\"test icb1\",\"Yorkshire\",,true,True,False,false,\"YES\",no,true,true,NO",
            "site2,\"test site 2\",\"123 test street\",\"01234 567890\",1.0,60.0,\"test icb2\",\"Yorkshire\",,true,True,False,false,\"YES\",no,true,true,NO",
            "site3,\"test site 3\",\"123 test street\",\"01234 567890\",1.0,60.0,\"test icb3\",\"Yorkshire\",,true,,False,\"oranges\",\"YES\",no,true,true,NO",
        ];

        var expectedSites = new SiteDocument[]
        {
            new()
            {
                OdsCode = "site1",
                Name = "test site 1",
                Address = "123 test street",
                PhoneNumber = "01234 567890",
                Location = new Location("Point", [1.0, 60.0]),
                DocumentType = "site",
                ReferenceNumberGroup = 0,
                IntegratedCareBoard = "test icb1",
                Region = "Yorkshire",
                AttributeValues =
                [
                    new AttributeValue("accessibility/accessible_toilet", "True"),
                    new AttributeValue("accessibility/braille_translation_service", "True"),
                    new AttributeValue("accessibility/disabled_car_parking", "False"),
                    new AttributeValue("accessibility/car_parking", "False"),
                    new AttributeValue("accessibility/induction_loop", "True"),
                    new AttributeValue("accessibility/sign_language_service", "False"),
                    new AttributeValue("accessibility/step_free_access", "True"),
                    new AttributeValue("accessibility/text_relay", "True"),
                    new AttributeValue("accessibility/wheelchair_access", "False")
                ]
            },
            new()
            {
                OdsCode = "site2",
                Name = "test site 2",
                Address = "123 test street",
                PhoneNumber = "01234 567890",
                Location = new Location("Point", [1.0, 60.0]),
                DocumentType = "site",
                ReferenceNumberGroup = 0,
                IntegratedCareBoard = "test icb2",
                Region = "Yorkshire",
                AttributeValues =
                [
                    new AttributeValue("accessibility/accessible_toilet", "True"),
                    new AttributeValue("accessibility/braille_translation_service", "True"),
                    new AttributeValue("accessibility/disabled_car_parking", "False"),
                    new AttributeValue("accessibility/car_parking", "False"),
                    new AttributeValue("accessibility/induction_loop", "True"),
                    new AttributeValue("accessibility/sign_language_service", "False"),
                    new AttributeValue("accessibility/step_free_access", "True"),
                    new AttributeValue("accessibility/text_relay", "True"),
                    new AttributeValue("accessibility/wheelchair_access", "False")
                ]
            },
            new()
            {
                OdsCode = "site3",
                Name = "test site 3",
                Address = "123 test street",
                PhoneNumber = "01234 567890",
                Location = new Location("Point", [1.0, 60.0]),
                DocumentType = "site",
                ReferenceNumberGroup = 0,
                IntegratedCareBoard = "test icb3",
                Region = "Yorkshire",
                AttributeValues =
                [
                    new AttributeValue("accessibility/accessible_toilet", "True"),
                    new AttributeValue("accessibility/braille_translation_service", "False"),
                    new AttributeValue("accessibility/disabled_car_parking", "False"),
                    new AttributeValue("accessibility/car_parking", "False"),
                    new AttributeValue("accessibility/induction_loop", "True"),
                    new AttributeValue("accessibility/sign_language_service", "False"),
                    new AttributeValue("accessibility/step_free_access", "True"),
                    new AttributeValue("accessibility/text_relay", "True"),
                    new AttributeValue("accessibility/wheelchair_access", "False")
                ]
            }
        };

        var input = BuildInputCsv(SitesHeader, inputRows);
        var actualSiteDocuments = new List<SiteDocument>();
        var mockFileOperations = new Mock<IFileOperations>();
        mockFileOperations.Setup(x => x.OpenText(It.IsAny<FileInfo>())).Returns(new StringReader(input));
        mockFileOperations.Setup(x => x.WriteDocument<SiteDocument>(It.IsAny<SiteDocument>(), It.IsAny<string>()))
            .Callback<SiteDocument, string>((doc, path) => actualSiteDocuments.Add(doc));

        var sut = new SiteDataImportHandler(mockFileOperations.Object);
        var report = await sut.ProcessFile(new FileInfo("test.csv"), new DirectoryInfo("out"));

        actualSiteDocuments.Should().BeEquivalentTo(expectedSites, opt => opt.Excluding(x => x.Id));

        //assert each site got a new distinct ID of type GUID
        actualSiteDocuments.Select(x => Guid.Parse(x.Id)).Distinct().Should().HaveCount(3);

        report.Count().Should().Be(3);
        report.Any(r => !r.Success).Should().BeFalse();
    }

    private string BuildInputCsv(string header, IEnumerable<string> dataLines)
    {
        var result = new StringBuilder(header);
        result.Append("\r\n");
        foreach (var line in dataLines)
        {
            result.Append($"{line}\r\n");
        }

        return result.ToString();
    }
}
