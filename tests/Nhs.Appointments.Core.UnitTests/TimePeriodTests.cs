namespace Nhs.Appointments.Core.UnitTests;

public class TimePeriodTests
{
    [Fact]
    public void Constructor_ThrowsException_WhenFromDateIsAfterUntil()
    {
        var fromDay = new DateTime(2000, 1, 10);
        var untilDay = new DateTime(2000, 1, 1);
        Assert.Throws<ArgumentException>(() => new TimePeriod(fromDay, untilDay)); 
    }

    [Fact]
    public void Split_ReturnsArrayWithSingleItem_WhenSplitIsCalledWithPointInTimeAfterTimePeriod()
    {
        var pointInTime = new DateTime(2077, 1, 17, 1, 0, 0);
        var timePeriod = new TimePeriod(new DateTime(2077, 1, 1, 9, 0, 0), new DateTime(2077, 1, 1, 12, 0, 0));
        
        var result = timePeriod.Split(pointInTime);

        Assert.Single(result);
        var first = result[0];
        Assert.Equal(timePeriod.From, first.From);
        Assert.Equal(timePeriod.Until, first.Until);
    }
    
    [Fact]
    public void Split_ReturnsArrayWithTwoNewTimePeriods_WhenSplitIsCalledWithinPointInTimeAfterTimePeriod()
    {
        var pointInTime = new DateTime(2077, 1, 1, 12, 0, 0);
        var timePeriod = new TimePeriod(new DateTime(2077, 1, 1, 9, 0, 0), new DateTime(2077, 1, 1, 17, 0, 0));
        
        var result = timePeriod.Split(pointInTime);

        Assert.Equal(2, result.Count());
        var first = result[0];
        var second = result[1];
        Assert.Equal(timePeriod.From, first.From);
        Assert.Equal(pointInTime, first.Until);
        Assert.Equal(pointInTime, second.From);
        Assert.Equal(timePeriod.Until, second.Until);
    }
}