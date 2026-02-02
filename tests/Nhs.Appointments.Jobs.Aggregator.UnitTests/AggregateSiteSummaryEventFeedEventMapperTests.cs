using Newtonsoft.Json.Linq;
using Nhs.Appointments.Jobs.ChangeFeed;

namespace Nhs.Appointments.Jobs.Aggregator.UnitTests;

public class AggregateSiteSummaryEventFeedEventMapperTests
{
    private readonly IFeedEventMapper<JObject, AggregateSiteSummaryEvent> _sut = new AggregateSiteSummaryEventFeedEventMapper();
    
    private JObject GenerateValidBookingDocument(DateTime dateTime, string site) => new()
    {
        { "docType", "booking" },
        { "site", site },
        { "from",  dateTime }
    };
    
    private JObject GenerateValidAvailabilityDocument(DateOnly date, string site) => new()
    {
        { "docType", "daily_availability" },
        { "site", site },
        { "from", date.ToString("yyyy-MM-dd") }
    };
    
    [Fact]
    public void MapToEvents_ReturnsEmptyList_WhenNoDocuments()
    {
        Assert.Empty(_sut.MapToEvents(new List<JObject>()));;
    }
    
    [Fact]
    public void MapToEvents_ReturnsDistinct_WhenDocumentsArentUnique()
    {
        const string site = "some-site";
        var jObjects = new List<JObject>()
        {
            GenerateValidAvailabilityDocument(new DateOnly(2025, 10, 1), site),
            GenerateValidAvailabilityDocument(new DateOnly(2025, 10, 1), site),
            GenerateValidBookingDocument(new DateTime(2025, 10, 1), site),
            GenerateValidBookingDocument(new DateTime(2025, 10, 1), site),
        };

        var expected = new List<AggregateSiteSummaryEvent>()
        {
            new(site, new DateOnly(2025, 10, 1))
        };
        
        Assert.Equivalent(_sut.MapToEvents(jObjects), expected);
    }
    
    [Fact]
    public void MapToEvents_ReturnsMultipleSites_WhenDocumentsArentUnique()
    {
        const string site1 = "some-site";
        const string site2 = "some-site";
        var jObjects = new List<JObject>()
        {
            GenerateValidAvailabilityDocument(new DateOnly(2025, 10, 1), site1),
            GenerateValidAvailabilityDocument(new DateOnly(2025, 10, 1), site2),
            GenerateValidBookingDocument(new DateTime(2025, 10, 1), site1),
            GenerateValidBookingDocument(new DateTime(2025, 10, 1), site2),
        };

        var expected = new List<AggregateSiteSummaryEvent>()
        {
            new(site1, new DateOnly(2025, 10, 1)),
            new(site2, new DateOnly(2025, 10, 1))
        };
        
        Assert.Equivalent(_sut.MapToEvents(jObjects), expected);
    }
    
    [Fact]
    public void MapToEvents_ReturnsMultipleDays_WhenDocumentsArentUnique()
    {
        const string site1 = "some-site";
        const string site2 = "some-site";
        var jObjects = new List<JObject>()
        {
            GenerateValidAvailabilityDocument(new DateOnly(2025, 10, 1), site1),
            GenerateValidAvailabilityDocument(new DateOnly(2025, 10, 1), site2),
            GenerateValidBookingDocument(new DateTime(2025, 10, 2), site1),
            GenerateValidBookingDocument(new DateTime(2025, 10, 2), site2),
        };

        var expected = new List<AggregateSiteSummaryEvent>()
        {
            new(site1, new DateOnly(2025, 10, 1)),
            new(site1, new DateOnly(2025, 10, 2)),
            new(site2, new DateOnly(2025, 10, 1)),
            new(site2, new DateOnly(2025, 10, 2))
        };
        
        Assert.Equivalent(_sut.MapToEvents(jObjects), expected);
    }
}
