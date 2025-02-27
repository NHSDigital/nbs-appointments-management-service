using DataExtract;
using FluentAssertions;
using Parquet.Schema;

namespace BookingDataExtracts.UnitTests;
public class DataFactoryTests
{
    [Fact]
    public void Field_HasCorrectName()
    {
        var dataFactory = new DataFactory<string, string>("test", s => s);
        dataFactory.Field.Name.Should().Be("test");
    }

    [Fact]
    public void CreateColumn_CreatesColumnCorrectly()
    {
        var dataFactory = new DataFactory<string, string>("test", s => new string(s.Reverse().ToArray()));
        var schema = new ParquetSchema(dataFactory.Field);
        var column = dataFactory.CreateColumn(new string[] { "one", "two" });
        column.Field.Name.Should().Be("test");
        column.Data.Should().BeEquivalentTo(new[] { "eno", "owt" });
    }

    [Fact]
    public void CreateColumn_ThrowsInvalidOperationException_WhenDataTypeIsIncorrect()
    {
        var dataFactory = new DataFactory<string, string>("test", s => new string(s.Reverse().ToArray()));
        var schema = new ParquetSchema(dataFactory.Field);
        var action = () => dataFactory.CreateColumn(new int[] { 1, 2 });
        action.Should().Throw<InvalidOperationException>();
    }
}
