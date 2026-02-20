using Nhs.Appointments.Core;

namespace Nhs.Appointments.Jobs.Aggregator.UnitTests;

public class LastUpdateByResolverTests
{
    private const string ApplicationName = "LastUpdateByResolverTests";
    private readonly ILastUpdatedByResolver _sut = new LastUpdatedByResolver(ApplicationName);
    
    [Fact]
    public void MapToEvents_ReturnsEmptyList_WhenNoDocuments()
    {
        Assert.Equal(ApplicationName, _sut.GetLastUpdatedBy());;
    }
}
