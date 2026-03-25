using Nhs.Appointments.Core.Metrics;

namespace Nhs.Appointments.Core.UnitTests.Metrics;

public class InMemoryMetricsRecorderTests
{
    private readonly InMemoryMetricsRecorder _sut = new();

    [Fact]
    public void RecordMetric_RecordsMetrics()
    {
        var random = new Random();
        var expectedValue1 = random.Next(1,1000);
        var generatedName1 = Guid.NewGuid().ToString();
        var testMetric = new TestMetric(generatedName1, expectedValue1);

        var generatedName2 = Guid.NewGuid().ToString();
        var expectedValue2 = Guid.NewGuid().ToString();
        var otherMetric = new OtherMetric(generatedName2, "myField", expectedValue2);

        _sut.RecordMetric(testMetric);
        _sut.RecordMetric(otherMetric);
        var expectedResults = new List<(string Path, IMetric Value)>
        {
            (generatedName1, testMetric),
            (generatedName2, otherMetric)
        };
        _sut.Metrics.Should().BeEquivalentTo(expectedResults);
    }

    [Fact]
    public void BeginScope_CreatesNestedScopes()
    {
        var generatedValue = Guid.NewGuid().ToString();
        var metric = new OtherMetric(generatedValue, "theField", Guid.NewGuid().ToString());

        using (_sut.BeginScope("Scope1"))
        {
            using(_sut.BeginScope("Scope2"))
            {
                _sut.RecordMetric(metric);
            }
        }
                
        var expectedResults = new List<(string Path, object Value)>
        {
            ($"Scope1/Scope2/{generatedValue}", metric),
        };
        _sut.Metrics.Should().BeEquivalentTo(expectedResults);
    }

    [Fact]
    public void BeginScope_PoppedScopes_AreHandledCorrectly()
    {
        var random = new Random();
        var expectedValue1 = random.Next(1, 1000);
        var generatedName1 = Guid.NewGuid().ToString();
        var testMetric = new TestMetric(generatedName1, expectedValue1);

        using (_sut.BeginScope("Scope1"))
        {
            _sut.RecordMetric(testMetric);
        }

        var generatedName2 = Guid.NewGuid().ToString();
        var expectedValue2 = Guid.NewGuid().ToString();
        var otherMetric = new OtherMetric(generatedName2, "myField", expectedValue2);
        
        using (_sut.BeginScope("Scope2"))
        {
            _sut.RecordMetric(otherMetric);
        }

        var expectedResults = new List<(string Path, object Value)>
        {
            ($"Scope1/{generatedName1}", testMetric),
            ($"Scope2/{generatedName2}", otherMetric),
        };

        _sut.Metrics.Should().BeEquivalentTo(expectedResults);
    }

    private class TestMetric(string name, int value) : IMetric
    {
        public string Name => name;

        public int Value => value;
    }

    private class OtherMetric(string name, string field, string value) : IMetric
    {
        public string Name => name;

        public string Field => field;

        public string Value => value;
    }
}
