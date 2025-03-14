namespace Nhs.Appointments.Core.UnitTests;
public class SessionInstanceExtensionsTests
{
    [Fact]
    public void GroupByConsecutive_ReturnsAll_WhenConsecutive1()
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

        var result = sessions.GroupByConsecutive(1);

        Assert.Equivalent(sessions, result);
    }

    [Fact]
    public void GroupByConsecutive_ReturnsOne_WhenConsecutive2() 
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

        var result = sessions.GroupByConsecutive(2);

        Assert.Single(result);
        Assert.Equal(5, result.First().Capacity);
    }

    [Fact]
    public void GroupByConsecutive_ReturnsThree_WhenConsecutive2()
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

        var result = sessions.GroupByConsecutive(2).ToArray();

        Assert.Equal(3, result.Count());
        Assert.Equal(5, result[0].Capacity);
        Assert.Equal(10, result[1].Capacity);
        Assert.Equal(12, result[2].Capacity);
    }

    [Fact]
    public void GroupByConsecutive_ReturnsOne_WhenConsecutive3()
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

        var result = sessions.GroupByConsecutive(3);

        Assert.Single(result);
        Assert.Equal(5, result.First().Capacity);
    }

    [Fact]
    public void GroupByConsecutive_ReturnsOne_WhenConsecutive4()
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

        var result = sessions.GroupByConsecutive(4);

        Assert.Single(result);
        Assert.Equal(2, result.First().Capacity);
    }

    [Fact]
    public void GroupByConsecutive_ReturnsOne_WhenConsecutive5()
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
            new (new TimePeriod(new DateTime(2025, 1, 1, 9, 15, 0), TimeSpan.FromMinutes(5)))
            {
                Capacity = 4,
                Services = ["test"]
            }
        };

        var result = sessions.GroupByConsecutive(5);

        Assert.Single(result);
        Assert.Equal(2, result.First().Capacity);
    }
}
