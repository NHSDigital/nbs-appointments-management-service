namespace Nhs.Appointments.Core.UnitTests;

public class ScopeTests
{
    [Fact]
    public void CanGetScopeValue()
    {
        var input = "site:some-site";
        var output = Scope.GetValue("site", input);
        Assert.NotNull(output);
        Assert.Equal("some-site", output);
    }

    [Fact]
    public void MustContainColon()
    {
        var input = "site some-site";
        var output = Scope.GetValue("site", input);
        Assert.True(string.IsNullOrEmpty(output));
    }

    [Fact]
    public void MustContainScopeType()
    {
        var input = "foo:some-value";
        var output = Scope.GetValue("site", input);
        Assert.True(string.IsNullOrEmpty(output));
    }

    [Fact]
    public void ValueCanBeEmpty()
    {
        var input = "site:";
        var output = Scope.GetValue("site", input);
        Assert.True(string.IsNullOrEmpty(output));
    }
}
