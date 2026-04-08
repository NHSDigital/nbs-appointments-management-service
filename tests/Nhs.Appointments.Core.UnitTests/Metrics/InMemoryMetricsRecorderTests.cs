using Microsoft.Extensions.Logging;
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
        var generatedPath1 = Guid.NewGuid().ToString();
        var testMetric = new TestMetric(generatedName1, generatedPath1, expectedValue1);

        var generatedName2 = Guid.NewGuid().ToString();
        var generatedPath2 = Guid.NewGuid().ToString();
        var expectedValue2 = Guid.NewGuid().ToString();
        var otherMetric = new OtherMetric(generatedName2, generatedPath2, "myField", expectedValue2);

        _sut.RecordMetric(testMetric);
        _sut.RecordMetric(otherMetric);
        var expectedResults = new List<IMetric>
        {
            testMetric,
            otherMetric
        };
        _sut.Metrics.Should().BeEquivalentTo(expectedResults);
    }

    private class TestMetric(string name, string path, int value) : IMetric
    {
        public string Name => name;

        public int Value => value;
    }

    private class OtherMetric(string name, string path, string field, string value) : IMetric
    {
        public string Name => name;

        public string Field => field;

        public string Value => value;
    }
}
