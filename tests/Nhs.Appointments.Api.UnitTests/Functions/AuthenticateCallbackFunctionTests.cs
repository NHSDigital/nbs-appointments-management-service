using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nhs.Appointments.Api.Functions;
using System.Text.Encodings.Web;

namespace Nhs.Appointments.Api.Tests.Functions;

public class AuthenticateCallbackFunctionTests
{
    [Fact]
    public void Test()
    {
        var context = new DefaultHttpContext();
        var defaultHttpRequest = context.Request;
        var url = UrlEncoder.Default.Encode("http://test.some.com");
        defaultHttpRequest.QueryString = new QueryString($"?code=123&state={url}");
        var result = AuthenticateCallbackFunction.Run(defaultHttpRequest);
        result.Should().BeOfType<RedirectResult>();
        (result as RedirectResult).Url.Should().Be("http://test.some.com?code=123");
    }
}
