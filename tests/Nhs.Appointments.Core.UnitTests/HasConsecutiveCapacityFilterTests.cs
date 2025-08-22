namespace Nhs.Appointments.Core.UnitTests;
public class HasConsecutiveCapacityFilterTests
{
    public HasConsecutiveCapacityFilterTests()
    {
        _sut = new HasConsecutiveCapacityFilter();
    }

    private IHasConsecutiveCapacityFilter _sut;

    private static DateTime AtTime(string timeInReadableFormat)
    {
        var hour = int.Parse(timeInReadableFormat.Split(':')[0]);
        var minute = int.Parse(timeInReadableFormat.Split(':')[1]);

        return new DateTime(2025, 4, 9, hour, minute, 0);
    }

    private static TimeSpan WithLength(int minutes) => TimeSpan.FromMinutes(minutes);

    [Fact]
    public void APPT_767_Scenario_One()
    {
        // In test scenario one for APPT-767, the following sessions exist:
        // "sessions": [{
        //     "from": "09:00",
        //     "until": "10:00",
        //     "services": ["RSV:Adult"],
        //     "slotLength": 5,
        //     "capacity": 1
        // },{
        //     "from": "09:00",
        //     "until": "10:00",
        //     "services": ["RSV:Adult"],
        //     "slotLength": 5,
        //     "capacity": 1
        // }],

        // This would be converted to slots as:
        var sessions = new List<SessionInstance>
        {
            new(new TimePeriod(AtTime("09:00"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:05"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:10"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:15"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:20"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:25"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:30"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:35"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:40"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:45"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:50"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:55"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:00"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:05"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:10"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:15"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:20"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:25"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:30"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:35"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:40"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:45"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:50"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:55"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] }
        };

        var slotsAfterFilteringByConsecutive = _sut.SessionHasConsecutiveSessions(sessions, 2);

        slotsAfterFilteringByConsecutive
            .Single(slot => slot.From.Hour is 9 & slot.From.Minute is 55)
            .Capacity.Should().Be(0);
    }

    [Fact]
    public void APPT_767_Scenario_Two()
    {
        // In test scenario two for APPT-767, the following sessions exist:
        // "sessions": [{
        //     "from": "09:00",
        //     "until": "10:00",
        //     "services": ["RSV:Adult"],
        //     "slotLength": 5,
        //     "capacity": 1
        // },{
        //     "from": "09:00",
        //     "until": "10:00",
        //     "services": ["RSV:Adult"],
        //     "slotLength": 5,
        //     "capacity": 2
        // }],

        // This would be converted to slots as:
        var sessions = new List<SessionInstance>
        {
            new(new TimePeriod(AtTime("09:00"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:05"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:10"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:15"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:20"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:25"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:30"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:35"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:40"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:45"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:50"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:55"), WithLength(5))) { Capacity = 1, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:00"), WithLength(5))) { Capacity = 2, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:05"), WithLength(5))) { Capacity = 2, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:10"), WithLength(5))) { Capacity = 2, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:15"), WithLength(5))) { Capacity = 2, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:20"), WithLength(5))) { Capacity = 2, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:25"), WithLength(5))) { Capacity = 2, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:30"), WithLength(5))) { Capacity = 2, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:35"), WithLength(5))) { Capacity = 2, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:40"), WithLength(5))) { Capacity = 2, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:45"), WithLength(5))) { Capacity = 2, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:50"), WithLength(5))) { Capacity = 2, Services = ["RSV:Adult"] },
            new(new TimePeriod(AtTime("09:55"), WithLength(5))) { Capacity = 2, Services = ["RSV:Adult"] }
        };

        var slotsAfterFilteringByConsecutive = _sut.SessionHasConsecutiveSessions(sessions, 2).ToList();

        slotsAfterFilteringByConsecutive.Count.Should().Be(12);
        slotsAfterFilteringByConsecutive.Count(blocks => blocks.Capacity == 3).Should().Be(11);

        slotsAfterFilteringByConsecutive
            .Single(slot => slot.From.Hour is 9 & slot.From.Minute is 55)
            .Capacity.Should().Be(0);
    }

    [Fact]
    public void SessionHasConsecutiveSessions_ReturnsAll_WhenConsecutive1()
    {
        var sessions = new List<SessionInstance>() 
        {
            new (new TimePeriod(new DateTime(2025, 1, 1, 9, 0, 0), TimeSpan.FromMinutes(5)))
            {
                Capacity = 5,
                Services = ["test"] 
            },
            new (new TimePeriod(new DateTime(2025, 1, 1, 9, 5, 0), TimeSpan.FromMinutes(5)))
            {
                Capacity = 10,
                Services = ["test"]
            }
        };

        var result = _sut.SessionHasConsecutiveSessions(sessions, 1);

        Assert.Equivalent(sessions, result);
    }

    [Fact]
    public void SessionHasConsecutiveSessions_ReturnsOne_WhenConsecutive2() 
    {
        var sessions = new List<SessionInstance>()
        {
            new (new TimePeriod(new DateTime(2025, 1, 1, 9, 0, 0), TimeSpan.FromMinutes(5)))
            {
                Capacity = 5,
                Services = ["test"]
            },
            new (new TimePeriod(new DateTime(2025, 1, 1, 9, 5, 0), TimeSpan.FromMinutes(5)))
            {
                Capacity = 10,
                Services = ["test"]
            }
        };

        var result = _sut.SessionHasConsecutiveSessions(sessions, 2);

        Assert.Equal(5, result.First().Capacity);
        Assert.Equal(0, result.Last().Capacity);
    }

    [Fact]
    public void SessionHasConsecutiveSessions_ReturnsThree_WhenConsecutive2()
    {
        var sessions = new List<SessionInstance>()
        {
            new (new TimePeriod(new DateTime(2025, 1, 1, 9, 0, 0), TimeSpan.FromMinutes(5)))
            {
                Capacity = 5,
                Services = ["test"]
            },
            new (new TimePeriod(new DateTime(2025, 1, 1, 9, 5, 0), TimeSpan.FromMinutes(5)))
            {
                Capacity = 10,
                Services = ["test"]
            },
            new (new TimePeriod(new DateTime(2025, 1, 1, 9, 10, 0), TimeSpan.FromMinutes(5)))
            {
                Capacity = 12,
                Services = ["test"]
            },
            new (new TimePeriod(new DateTime(2025, 1, 1, 9, 15, 0), TimeSpan.FromMinutes(5)))
            {
                Capacity = 15,
                Services = ["test"]
            }
        };

        var result = _sut.SessionHasConsecutiveSessions(sessions, 2).ToArray();

        Assert.Equal(4, result.Count());
        Assert.Equal(5, result[0].Capacity);
        Assert.Equal(10, result[1].Capacity);
        Assert.Equal(12, result[2].Capacity);
        Assert.Equal(0, result[3].Capacity);
    }

    [Fact]
    public void SessionHasConsecutiveSessions_ReturnsOne_WhenConsecutive3()
    {
        var sessions = new List<SessionInstance>()
        {
            new (new TimePeriod(new DateTime(2025, 1, 1, 9, 0, 0), TimeSpan.FromMinutes(5)))
            {
                Capacity = 5,
                Services = ["test"]
            },
            new (new TimePeriod(new DateTime(2025, 1, 1, 9, 5, 0), TimeSpan.FromMinutes(5)))
            {
                Capacity = 10,
                Services = ["test"]
            },
            new (new TimePeriod(new DateTime(2025, 1, 1, 9, 10, 0), TimeSpan.FromMinutes(5)))
            {
                Capacity = 12,
                Services = ["test"]
            }
        };

        var result = _sut.SessionHasConsecutiveSessions(sessions, 3).ToArray();

        Assert.Equal(5, result[0].Capacity);
        Assert.Equal(0, result[1].Capacity);
        Assert.Equal(0, result[2].Capacity);
    }

    [Fact]
    public void SessionHasConsecutiveSessions_ReturnsOne_WhenConsecutive4()
    {
        var sessions = new List<SessionInstance>()
        {
            new (new TimePeriod(new DateTime(2025, 1, 1, 9, 0, 0), TimeSpan.FromMinutes(5)))
            {
                Capacity = 5,
                Services = ["test"]
            },
            new (new TimePeriod(new DateTime(2025, 1, 1, 9, 5, 0), TimeSpan.FromMinutes(5)))
            {
                Capacity = 10,
                Services = ["test"]
            },
            new (new TimePeriod(new DateTime(2025, 1, 1, 9, 10, 0), TimeSpan.FromMinutes(5)))
            {
                Capacity = 12,
                Services = ["test"]
            },
            new (new TimePeriod(new DateTime(2025, 1, 1, 9, 15, 0), TimeSpan.FromMinutes(5)))
            {
                Capacity = 2,
                Services = ["test"]
            }
        };

        var result = _sut.SessionHasConsecutiveSessions(sessions, 4).ToArray();

        Assert.Equal(2, result[0].Capacity);
        Assert.Equal(0, result[1].Capacity);
        Assert.Equal(0, result[2].Capacity);
        Assert.Equal(0, result[3].Capacity);
    }

    [Fact]
    public void SessionHasConsecutiveSessions_ReturnsOne_WhenConsecutive5()
    {
        var sessions = new List<SessionInstance>()
        {
            new (new TimePeriod(new DateTime(2025, 1, 1, 9, 0, 0), TimeSpan.FromMinutes(5)))
            {
                Capacity = 5,
                Services = ["test"]
            },
            new (new TimePeriod(new DateTime(2025, 1, 1, 9, 5, 0), TimeSpan.FromMinutes(5)))
            {
                Capacity = 10,
                Services = ["test"]
            },
            new (new TimePeriod(new DateTime(2025, 1, 1, 9, 10, 0), TimeSpan.FromMinutes(5)))
            {
                Capacity = 12,
                Services = ["test"]
            },
            new (new TimePeriod(new DateTime(2025, 1, 1, 9, 15, 0), TimeSpan.FromMinutes(5)))
            {
                Capacity = 2,
                Services = ["test"]
            },
            new(new TimePeriod(new DateTime(2025, 1, 1, 9, 20, 0), TimeSpan.FromMinutes(5)))
            {
                Capacity = 4,
                Services = ["test"]
            }
        };

        var result = _sut.SessionHasConsecutiveSessions(sessions, 5).ToArray();

        Assert.Equal(2, result[0].Capacity);
        Assert.Equal(0, result[1].Capacity);
        Assert.Equal(0, result[2].Capacity);
        Assert.Equal(0, result[3].Capacity);
        Assert.Equal(0, result[4].Capacity);
    }
}
