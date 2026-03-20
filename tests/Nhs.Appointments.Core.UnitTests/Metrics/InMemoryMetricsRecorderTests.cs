using Nhs.Appointments.Core.Metrics;

namespace Nhs.Appointments.Core.UnitTests.Metrics;

public class InMemoryMetricsRecorderTests
{
    private readonly InMemoryMetricsRecorder _sut = new();

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
    public void DifferentMetricsAreRecorded_RecordMetricIsCalled_TheMetricsAreRecordedFaithfully()
    {
        var generatedValue = Guid.NewGuid().ToString();
        var mockMetric = new Mock<IMetric>();
        mockMetric.SetupGet(x => x.Name).Returns(generatedValue);

        _sut.RecordMetric("Test", 1);
        _sut.RecordMetric("IMetric", mockMetric.Object);
        var expectedResults = new List<(string Path, object Value)>
        {
            ("Test", 1),
            ("IMetric", mockMetric.Object)
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
                
        var expectedResults = new List<(string Path, object Value)>
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

        var generatedValue = Guid.NewGuid().ToString();
        var mockMetric = new Mock<IMetric>();
        mockMetric.SetupGet(x => x.Name).Returns(generatedValue);
        using (_sut.BeginScope("Scope2"))
        {
            _sut.RecordMetric("IMetric", mockMetric.Object);
        }

        var expectedResults = new List<(string Path, object Value)>
        {
            ("Scope1/Test", 1),
            ("Scope2/IMetric", mockMetric.Object),
        };
        _sut.Metrics.Should().BeEquivalentTo(expectedResults);
    }

}

