namespace Nhs.Appointments.Core.UnitTests;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class MockedFeatureToggleAttribute(string toggle, bool value) : Attribute
{
    public string Name { get; private set; } = toggle;
    public bool Value { get; private set; } = value;
}
