using FluentAssertions;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Json;
using System.Text.Json;

namespace Nhs.Appointments.Api.Tests.Json
{
    public class JsonToObjectSchemaValidationTests
    {
        [Fact]
        public void ValidateConversion_ReturnsError_WhenJsonIsImproperlyFormatted()
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<Object[]>("{\"prop\": }");
            results.Count.Should().Be(1);
            results[0].Property.Should().Be("document");
            results[0].Message.Should().Be("The json is not properly formatted");
        }

        [Theory]
        [InlineData("\"string\"", JsonValueKind.String)]
        [InlineData("32", JsonValueKind.Number)]
        [InlineData("true", JsonValueKind.True)]
        [InlineData("null", JsonValueKind.Null)]
        [InlineData("{}", JsonValueKind.Object)]
        public void ValidateConversion_ReturnsError_WhenArrayExpectedButNotRecieved(string json, JsonValueKind jsonKind)
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<Object[]>(json);
            results.Count.Should().Be(1);
            results[0].Property.Should().BeEmpty();
            results[0].Message.Should().Be($"Expected an array but got {jsonKind}");
        }

        [Theory]
        [InlineData("\"string\"", JsonValueKind.String)]
        [InlineData("32", JsonValueKind.Number)]
        [InlineData("true", JsonValueKind.True)]
        [InlineData("null", JsonValueKind.Null)]
        [InlineData("[]", JsonValueKind.Array)]
        public void ValidateConversion_ReturnsError_WhenObjectExpectedButNotRecieved(string json, JsonValueKind jsonKind)
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<Object>(json);
            results.Count.Should().Be(1);
            results[0].Property.Should().BeEmpty();
            results[0].Message.Should().StartWith($"Expected an object but got {jsonKind}");
        }

        [Fact]        
        public void ValidateConversion_ReturnsError_WhenArrayContainsNonMatchingTypes()
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<string[]>("[\"1\",2,3]");
            results.Count.Should().Be(2);
            results[0].Property.Should().Be("root[1]");
            results[0].Message.Should().Be("Expected a string but found Number");
            results[1].Property.Should().Be("root[2]");
            results[1].Message.Should().Be("Expected a string but found Number");
        }

        [Fact]
        public void ValidateConversion_ReturnsNoErrors_WhenArrayIsCorrect()
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<int[]>("[1,2,3]");
            results.Count.Should().Be(0);            
        }

        [Fact]
        public void ValidateConversion_ReturnsErrors_WhenFloatAreInAnIntArray()
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<int[]>("[1,2.4,3]");
            results.Count.Should().Be(1);
            results[0].Property.Should().Be("root[1]");
            results[0].Message.Should().Be("Expected an integer but found a floating point number");
        }

        [Theory]
        [InlineData("{\"MyProp\": 23}", JsonValueKind.Number)]
        [InlineData("{\"MyProp\": 23.4}", JsonValueKind.Number)]
        [InlineData("{\"MyProp\": true}", JsonValueKind.True)]
        [InlineData("{\"MyProp\": false}", JsonValueKind.False)]
        [InlineData("{\"MyProp\": {}}", JsonValueKind.Object)]
        [InlineData("{\"MyProp\": [1,2,3]}", JsonValueKind.Array)]        
        public void ValidateConversion_ReturnsErrors_WhenStringPropertyIsIncorrect(string json, JsonValueKind jsonValueKind)
        {
            
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithStringProperty>(json);
            results.Count.Should().Be(1);
            results[0].Property.Should().Be("MyProp");
            results[0].Message.Should().Be("Expected a string but found " + jsonValueKind);
        }

        [Fact]
        public void ValidateConversion_ReturnsNoErrors_WhenStringPropertyIsCorrect()
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithStringProperty>("{\"MyProp\": \"text\"}");
            results.Count.Should().Be(0);
        }

        [Theory]
        [InlineData("{\"MyProp\": \"hello\"}", JsonValueKind.String)]        
        [InlineData("{\"MyProp\": true}", JsonValueKind.True)]
        [InlineData("{\"MyProp\": false}", JsonValueKind.False)]
        [InlineData("{\"MyProp\": {}}", JsonValueKind.Object)]
        [InlineData("{\"MyProp\": [1,2,3]}", JsonValueKind.Array)]
        public void ValidateConversion_ReturnsErrors_WhenIntPropertyIsIncorrect(string json, JsonValueKind jsonValueKind)
        {

            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithIntProperty>(json);
            results.Count.Should().Be(1);
            results[0].Property.Should().Be("MyProp");
            results[0].Message.Should().Be("Expected a number but found " + jsonValueKind);
        }

        [Fact]
        public void ValidateConversion_ReturnsErrors_WhenIntPropertyRecievesFloat()
        {

            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithIntProperty>("{\"MyProp\": 2.3}");
            results.Count.Should().Be(1);
            results[0].Property.Should().Be("MyProp");
            results[0].Message.Should().Be("Expected an integer but found a floating point number");
        }

        [Fact]
        public void ValidateConversion_ReturnsNoErrors_WhenIntPropertyIsCorrect()
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithIntProperty>("{\"MyProp\": 2}");
            results.Count.Should().Be(0);
        }

        [Theory]
        [InlineData("{\"MyProp\": \"hello\"}", JsonValueKind.String)]
        [InlineData("{\"MyProp\": 22}", JsonValueKind.Number)]
        [InlineData("{\"MyProp\": 22.3}", JsonValueKind.Number)]
        [InlineData("{\"MyProp\": {}}", JsonValueKind.Object)]
        [InlineData("{\"MyProp\": [1,2,3]}", JsonValueKind.Array)]
        [InlineData("{\"MyProp\": null}", JsonValueKind.Null)]
        public void ValidateConversion_ReturnsErrors_WhenBoolPropertyIsIncorrect(string json, JsonValueKind jsonValueKind)
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithBoolProperty>(json);
            results.Count.Should().Be(1);
            results[0].Property.Should().Be("MyProp");
            results[0].Message.Should().Be("Expected a boolean value but found " + jsonValueKind);
        }

        [Theory]
        [InlineData("{\"MyProp\": true}")]
        [InlineData("{\"MyProp\": false}")]
        public void ValidateConversion_ReturnsNoErrors_WhenBoolPropertyIsCorrect(string json)
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithBoolProperty>(json);
            results.Count.Should().Be(0);
        }

        [Theory]        
        [InlineData("{\"MyProp\": 22}")]
        [InlineData("{\"MyProp\": 22.3}")]
        [InlineData("{\"MyProp\": {}}")]
        [InlineData("{\"MyProp\": [1,2,3]}")]
        [InlineData("{\"MyProp\": null}")]
        [InlineData("{\"MyProp\": true}")]
        [InlineData("{\"MyProp\": false}")]
        public void ValidateConversion_ReturnsErrors_WhenTimeOnlyPropertyIsIncorrect(string json)
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithTimeOnlyProperty>(json);
            results.Count.Should().Be(1);
            results[0].Property.Should().Be("MyProp");
            results[0].Message.Should().Be($"Times should be provided as a string in the following format {DateTimeFormats.TimeOnly}");
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid")]
        [InlineData("43:00")]
        [InlineData("12:86")]
        public void ValidateConversion_ReturnsErrors_WhenTimeOnlyPropertyFormatIsIncorrect(string timeData)
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithTimeOnlyProperty>("{\"MyProp\": \""+timeData+"\"}");
            results.Count.Should().Be(1);
            results[0].Property.Should().Be("MyProp");
            results[0].Message.Should().Be($"Times should be provided as a string in the following format {DateTimeFormats.TimeOnly}");
        }

        [Fact]
        public void ValidateConversion_ReturnsNoErrors_WhenTimeOnlyPropertyIsCorrect()
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithTimeOnlyProperty>("{\"MyProp\": \"23:45\"}");
            results.Count.Should().Be(0);            
        }

        [Theory]
        [InlineData("{\"MyProp\": 22}")]
        [InlineData("{\"MyProp\": 22.3}")]
        [InlineData("{\"MyProp\": {}}")]
        [InlineData("{\"MyProp\": [1,2,3]}")]
        [InlineData("{\"MyProp\": null}")]
        [InlineData("{\"MyProp\": true}")]
        [InlineData("{\"MyProp\": false}")]
        public void ValidateConversion_ReturnsErrors_WhenDateOnlyPropertyIsIncorrect(string json)
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithDateOnlyProperty>(json);
            results.Count.Should().Be(1);
            results[0].Property.Should().Be("MyProp");
            results[0].Message.Should().Be($"Dates should be provided as a string in the following format {DateTimeFormats.DateOnly}");
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid")]
        [InlineData("1999-23-01")]
        [InlineData("1999-02-30")]
        [InlineData("1999-12-33")]
        public void ValidateConversion_ReturnsErrors_WhenDateOnlyPropertyFormatIsIncorrect(string dateData)
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithDateOnlyProperty>("{\"MyProp\": \""+dateData+"\"}");
            results.Count.Should().Be(1);
            results[0].Property.Should().Be("MyProp");
            results[0].Message.Should().Be($"Dates should be provided as a string in the following format {DateTimeFormats.DateOnly}");
        }

        [Fact]
        public void ValidateConversion_ReturnsNoErrors_WhenDateOnlyPropertyIsCorrect()
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithDateOnlyProperty>("{\"MyProp\": \"2077-01-01\"}");
            results.Count.Should().Be(0);
        }

        [Theory]
        [InlineData("{\"MyProp\": 22}")]
        [InlineData("{\"MyProp\": 22.3}")]
        [InlineData("{\"MyProp\": {}}")]
        [InlineData("{\"MyProp\": [1,2,3]}")]
        [InlineData("{\"MyProp\": null}")]
        [InlineData("{\"MyProp\": true}")]
        [InlineData("{\"MyProp\": false}")]
        public void ValidateConversion_ReturnsErrors_WhenDateTimePropertyIsIncorrect(string json)
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithDateTimeProperty>(json);
            results.Count.Should().Be(1);
            results[0].Property.Should().Be("MyProp");
            results[0].Message.Should().Be($"Date time data should be provided as a string in the following format {DateTimeFormats.DateTime}");
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid")]
        [InlineData("1999-23-01 12:00")]
        [InlineData("1999-02-30 12:00")]
        [InlineData("1999-12-33 12:00")]
        [InlineData("2077-01-01 43:00")]
        [InlineData("2077-01-01 12:86")]
        public void ValidateConversion_ReturnsErrors_WhenDateTimePropertyFormatIsIncorrect(string dateTimeData)
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithDateTimeProperty>("{\"MyProp\": \""+dateTimeData+"\"}");
            results.Count.Should().Be(1);
            results[0].Property.Should().Be("MyProp");
            results[0].Message.Should().Be($"Date time data should be provided as a string in the following format {DateTimeFormats.DateTime}");
        }

        [Fact]
        public void ValidateConversion_ReturnsNoErrors_WhenDateTimePropertyIsCorrect()
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithDateTimeProperty>("{\"MyProp\": \"2077-01-01 12:00\"}");
            results.Count.Should().Be(0);
        }

        [Theory]
        [InlineData("{\"MyProp\": \"hello\"}", JsonValueKind.String)]
        [InlineData("{\"MyProp\": 22}", JsonValueKind.Number)]
        [InlineData("{\"MyProp\": 22.3}", JsonValueKind.Number)]        
        [InlineData("{\"MyProp\": [1,2,3]}", JsonValueKind.Array)]
        [InlineData("{\"MyProp\": true}", JsonValueKind.True)]
        [InlineData("{\"MyProp\": false}", JsonValueKind.False)]
        [InlineData("{\"MyProp\": null}", JsonValueKind.Null)]
        public void ValidateConversion_ReturnsErrors_WhenObjectPropertyIsIncorrect(string json, JsonValueKind jsonValueKind)
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithObjectProperty>(json);
            results.Count.Should().Be(1);
            results[0].Property.Should().Be("MyProp");
            results[0].Message.Should().Be($"Expected an object but found {jsonValueKind}");
        }

        [Fact]
        public void ValidateConversion_ReturnsErrors_WhenObjectPropertyWrongSchema()
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithObjectProperty>("{\"MyProp\": {\"NestedProp\": \"hello\"}}");
            results.Count.Should().Be(1);
            results[0].Property.Should().Be("NestedProp");
            results[0].Message.Should().Be($"Expected a boolean value but found String");
        }

        [Fact]
        public void ValidateConversion_ReturnsNoErrors_WhenObjectPropertyIsCorrect()
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithObjectProperty>("{\"MyProp\": {\"NestedProp\": true}}");
            results.Count.Should().Be(0);
        }

        [Theory]
        [InlineData("{\"MyProp\": \"hello\"}", JsonValueKind.String)]
        [InlineData("{\"MyProp\": 22}", JsonValueKind.Number)]
        [InlineData("{\"MyProp\": 22.3}", JsonValueKind.Number)]
        [InlineData("{\"MyProp\": {}}", JsonValueKind.Object)]
        [InlineData("{\"MyProp\": true}", JsonValueKind.True)]
        [InlineData("{\"MyProp\": false}", JsonValueKind.False)]
        [InlineData("{\"MyProp\": null}", JsonValueKind.Null)]
        public void ValidateConversion_ReturnsErrors_WhenArrayPropertyIsIncorrect(string json, JsonValueKind jsonValueKind)
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithArrayProperty>(json);
            results.Count.Should().Be(1);
            results[0].Property.Should().Be("MyProp");
            results[0].Message.Should().Be($"Expected an array but found {jsonValueKind}");
        }

        [Fact]
        public void ValidateConversion_ReturnsNoErrors_WhenArrayPropertyIsCorrect()
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithArrayProperty>("{\"MyProp\": [1,2,3]}");
            results.Count.Should().Be(0);
        }

        [Theory]
        [InlineData("{\"MyProp\": 22}", JsonValueKind.Number)]
        [InlineData("{\"MyProp\": 22.3}", JsonValueKind.Number)]
        [InlineData("{\"MyProp\": []}", JsonValueKind.Array)]
        [InlineData("{\"MyProp\": {}}", JsonValueKind.Object)]
        [InlineData("{\"MyProp\": true}", JsonValueKind.True)]
        [InlineData("{\"MyProp\": false}", JsonValueKind.False)]
        [InlineData("{\"MyProp\": null}", JsonValueKind.Null)]
        public void ValidateConversion_ReturnsErrors_WhenEnumPropertyIsIncorrect(string json, JsonValueKind jsonValueKind)
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithEnumProperty>(json);
            results.Count.Should().Be(1);
            results[0].Property.Should().Be("MyProp");
            results[0].Message.Should().Be($"Expected a string value but found {jsonValueKind}");
        }

        [Fact]
        public void ValidateConversion_ReturnsErrors_WhenEnumPropertyIsIntegerSentAsString()
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithEnumProperty>("{\"MyProp\": \"1\"}");
            results.Count.Should().Be(1);
            results[0].Property.Should().Be("MyProp");
            results[0].Message.Should().Be($"1 is not a valid value");
        }

        [Theory]
        [InlineData(TestEnum.Alpha)]
        [InlineData(TestEnum.Beta)]
        [InlineData(TestEnum.Gamma)]
        public void ValidateConversion_ReturnsNoErrors_WhenEnumPropertyIsCorrect(TestEnum enumData)
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithEnumProperty>("{\"MyProp\": \"" + enumData+ "\"}");
            results.Count.Should().Be(0);
        }

        [Fact]
        public void ValidateConversion_ReturnsErrors_WhenEnumPropertyValueNotInRange()
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithEnumProperty>("{\"MyProp\": \"Omega\"}");
            results.Count.Should().Be(1);
            results[0].Property.Should().Be("MyProp");
            results[0].Message.Should().Be("Omega is not a valid value");
        }

        [Fact]
        public void ValidateConversion_ReturnsErrors_WhenPropertyIsSuppliedThatDoesNotExist()
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithIntProperty>("{\"MyProp\": 1, \"ExtraProp\": 2}");
            results.Count.Should().Be(1);
            results[0].Property.Should().Be("ExtraProp");
            results[0].Message.Should().Be("The property does not exist on the request type");
        }

        [Fact]
        public void ValidateConversion_DoesNotValidateNestedObjects_WhenDynamicDataIsExpected()
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithDyamicProperty>("{\"DynamicProp\": {\"Prop\": 2} }");
            results.Count.Should().Be(0);            
        }

        [Fact]
        public void ValidateConversion_ReturnsErrors_WhenRequiredPropertyIsNotProvided()
        {
            var results = JsonToObjectSchemaValidation.ValidateConversion<ClassWithRequiredProperty>("{}");
            results.Count.Should().Be(1);
            results[0].Property.Should().Be("MyProp");
            results[0].Message.Should().Be("This property is required but was not provided");
        }

        public class ClassWithDateTimeProperty
        {
            public DateTime MyProp { get; set; }
        }

        public class ClassWithDateOnlyProperty
        {
            public DateOnly MyProp { get; set; }
        }

        public class ClassWithTimeOnlyProperty
        {
            public TimeOnly MyProp { get; set; }
        }

        public class ClassWithStringProperty
        {
            public string MyProp { get; set; }
        }

        public class ClassWithIntProperty
        {
            public int MyProp { get; set; }
        }

        public class ClassWithBoolProperty
        {
            public bool MyProp { get; set; }
        }

        public class ClassWithObjectProperty
        {
            public NestedClass MyProp { get; set; }
        }

        public class ClassWithArrayProperty
        {
            public int[] MyProp { get; set; }
        }

        public class NestedClass
        {
            public bool NestedProp { get; set; }
        }

        public enum TestEnum { Alpha, Beta, Gamma }

        public class ClassWithEnumProperty
        {
            public TestEnum MyProp { get; set; }
        }

        public class ClassWithDyamicProperty
        {
            public Object DynamicProp { get; set; }
        }

        public class ClassWithRequiredProperty
        {
            [JsonProperty(Required = Required.Always)]
            public string MyProp { get; set; }
        }
    }
}
