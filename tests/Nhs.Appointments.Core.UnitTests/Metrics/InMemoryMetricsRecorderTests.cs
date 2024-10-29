namespace Nhs.Appointments.Core.UnitTests.Metrics;

public class InMemoryMetricsRecorderTests
{
    private readonly InMemoryMetricsRecorder _sut = new InMemoryMetricsRecorder();

    [Fact]
    public void RecordMetric_RecordsMetrics()
    {            
        _sut.RecordMetric("Test", 1);
        _sut.RecordMetric("Other", 2);
        var expectedResults = new List<(string Path, double Value)>
        {
            ("Test", 1),
            ("Other", 2)
        };
        _sut.Metrics.Should().BeEquivalentTo(expectedResults);
    }

    [Fact]
    public void BeginScope_CreatesNestedScopes()
    {
        using(_sut.BeginScope("Scope1"))
        {
            using(_sut.BeginScope("Scope2"))
            {
                _sut.RecordMetric("Test", 1);
            }
        }
                
        var expectedResults = new List<(string Path, double Value)>
        {
            ("Scope1/Scope2/Test", 1),
        };
        _sut.Metrics.Should().BeEquivalentTo(expectedResults);
    }

    [Fact]
    public void BeginScope_PoppedScopes_AreHandledCorrectly()
    {
        using (_sut.BeginScope("Scope1"))
        {
            _sut.RecordMetric("Test", 1);            
        }

        using (_sut.BeginScope("Scope2"))
        {
            _sut.RecordMetric("Other", 1);
        }

        var expectedResults = new List<(string Path, double Value)>
        {
            ("Scope1/Test", 1),
            ("Scope2/Other", 1),
        };
        _sut.Metrics.Should().BeEquivalentTo(expectedResults);
    }

}

