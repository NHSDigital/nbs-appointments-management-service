using BookingsDataExtracts;
using DataExtract.Documents;
using FluentAssertions;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace BookingDataExtracts.UnitTests;

public class BookingDataConverterTests
{
    [Fact]
    public void ExtractAppointmentDateTime_GetsCorrectData()
    {
        var testDocument = new NbsBookingDocument
        {
            From = new DateTime(2025, 01, 02, 15, 23, 00)
        };
        var result = BookingDataConverter.ExtractAppointmentDateTime(testDocument);
        result.Should().Be("2025-01-02T15:23:00+00:00");
    }

    [Theory]
    [InlineData(AppointmentStatus.Booked, "B")]
    [InlineData(AppointmentStatus.Cancelled, "C")]
    public void ExtractAppointmentStatus_ConvertsStatus(AppointmentStatus status, string expectedData)
    {
        var testDocument = new NbsBookingDocument
        {
            Status = status
        };
        var result = BookingDataConverter.ExtractAppointmentStatus(testDocument);
        result.Should().Be(expectedData);
    }

    [Theory]
    [InlineData(AppointmentStatus.Booked, "2025-01-01 14:44", "NULL")]
    [InlineData(AppointmentStatus.Cancelled, "2025-01-01 14:44", "2025-01-01T14:44:00+00:00")]
    public void ExtractCancelledDateTime_GetDateTime_OnlyWhenCancelled(AppointmentStatus status, string statusDateTime, string expectedData)
    {
        var testDocument = new NbsBookingDocument
        {
            Status = status,
            StatusUpdated = DateTimeOffset.ParseExact(statusDateTime, "yyyy-MM-dd HH:mm", null)
        };
        var result = BookingDataConverter.ExtractCancelledDateTime(testDocument);
        result.Should().Be(expectedData);
    }

    [Fact]
    public void ExtractCreatedDateTime_FormatsDate()
    {
        var testDocument = new NbsBookingDocument
        {
            Created = new DateTime(2025, 01, 02, 13, 14, 15)
        };
        var result = BookingDataConverter.ExtractCreatedDateTime(testDocument);
        result.Should().Be("2025-01-02T13:14:15+00:00");
    }

    [Fact]
    public void ExtractDateOfBirth_FormatsDate()
    {
        var testDocument = new NbsBookingDocument
        {
            AttendeeDetails = new()
            {
                DateOfBirth = new DateOnly(1977, 1, 24)
            }
        };
        var result = BookingDataConverter.ExtractDateOfBirth(testDocument);
        result.Should().Be("1977-01-24");
    }

    [Theory]
    [InlineData("1", "ODS_01")]
    [InlineData("2", "ODS_02")]
    public void ExtractOdsCode_ReturnsOdsCodeFromDocument(string siteId, string expectedData)
    {
        var testDocument = new NbsBookingDocument
        {
            Site = siteId
        };
        var converter = new BookingDataConverter(TestSites);
        var result = converter.ExtractOdsCode(testDocument);
        result.Should().Be(expectedData);
    }

    [Theory]
    [InlineData("SelfReferred", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("UnexpectedValue", false)]
    public void ExtractSelfReferral_ConvertsData(string selfReferralValue, bool expectedData)
    {
        var testDocument = new NbsBookingDocument
        {
            AdditionalData = new()
            {
                ReferralType = selfReferralValue
            }
        };
        var result = BookingDataConverter.ExtractSelfReferral(testDocument);
        result.Should().Be(expectedData);
    }

    [Fact]
    public void ExtractService_GetsCorrectData()
    {
        var testDocument = new NbsBookingDocument
        {
            Service = "Test:Service"
        };
        var result = BookingDataConverter.ExtractService(testDocument);
        result.Should().Be("Test:Service");
    }

    [Theory]
    [InlineData(false, false, "NBS_Website")]
    [InlineData(true, false, "NHS_App")]
    [InlineData(true, true, "NHS_App")]
    [InlineData(false, true, "NHS_Call_Centre")]
    public void ExtractSource_GetsCorrectData(bool isNhsApp, bool isCallCentre, string expectedData)
    {
        var testDocument = new NbsBookingDocument
        {
            AdditionalData = new()
            {
                IsAppBooking = isNhsApp,
                IsCallCentreBooking = isCallCentre
            }
        };
        var result = BookingDataConverter.ExtractSource(testDocument);
        result.Should().Be(expectedData);
    }

    [Fact]
    public void ExtractBookingReference_GetsCorrectData()
    {
        var testDocument = new NbsBookingDocument
        {
            Reference = "01-02-1234567"
        };
        var result = BookingDataConverter.ExtractBookingReference(testDocument);
        result.Should().Be("01-02-1234567");
    }

    [Fact]
    public void ExtractNhsNumber_GetsNhsNumber()
    {
        var testDocument = new NbsBookingDocument
        {
            AttendeeDetails = new()
            {
                NhsNumber = "123456778"
            }
        };

        var result = BookingDataConverter.ExtractNhsNumber(testDocument);
        result.Should().Be("123456778");
    }

    [Theory]
    [InlineData("1", "Site One")]
    [InlineData("2", "Site Two")]
    public void ExtractSiteName_GetsName_FromCorrectSite(string site, string expectedData)
    {
        var testDocument = new NbsBookingDocument
        {
            Site = site
        };
        var converter = new BookingDataConverter(TestSites);
        var result = converter.ExtractSiteName(testDocument);
        result.Should().Be(expectedData);
    }

    [Theory]
    [InlineData("1", "RGN01")]
    [InlineData("2", "RGN02")]
    public void ExtractRegion_GetsRegion_FromCorrectSite(string site, string expectedData)
    {
        var testDocument = new NbsBookingDocument
        {
            Site = site
        };
        var converter = new BookingDataConverter(TestSites);
        var result = converter.ExtractRegion(testDocument);
        result.Should().Be(expectedData);
    }

    [Theory]
    [InlineData("1", "ICB01")]
    [InlineData("2", "ICB02")]
    public void ExtractICB_GetsICB_FromCorrectSite(string site, string expectedData)
    {
        var testDocument = new NbsBookingDocument
        {
            Site = site
        };
        var converter = new BookingDataConverter(TestSites);
        var result = converter.ExtractICB(testDocument);
        result.Should().Be(expectedData);
    }

    [Theory]
    [InlineData("1", 0.1)]
    [InlineData("2", 0.2)]
    public void ExtractLatitude_GetsLatitude_FromCorrectSite(string site, double expectedData)
    {
        var testDocument = new NbsBookingDocument
        {
            Site = site
        };
        var converter = new BookingDataConverter(TestSites);
        var result = converter.ExtractLatitude(testDocument);
        result.Should().Be(expectedData);
    }

    [Theory]
    [InlineData("1", 1.0)]
    [InlineData("2", 2.0)]
    public void ExtractLongitude_GetsLongitude_FromCorrectSite(string site, double expectedData)
    {
        var testDocument = new NbsBookingDocument
        {
            Site = site
        };
        var converter = new BookingDataConverter(TestSites);
        var result = converter.ExtractLongitude(testDocument);
        result.Should().Be(expectedData);
    }


    private IEnumerable<SiteDocument> TestSites => new[]
    {
        new SiteDocument
        {
            Id = "1",
            Name = "Site One",
            OdsCode = "ODS_01",
            IntegratedCareBoard = "ICB01",
            Region = "RGN01",
            Location = new Location("Point", new []{1.0, 0.1 })
        },
        new SiteDocument
        {
            Id = "2",
            Name = "Site Two",
            OdsCode = "ODS_02",
            IntegratedCareBoard = "ICB02",
            Region = "RGN02",
            Location = new Location("Point", new []{2.0, 0.2 })
        }
    };
}
