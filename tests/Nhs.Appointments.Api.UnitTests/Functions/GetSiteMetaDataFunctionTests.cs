﻿using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Tests.Functions;

public class GetSiteMetaDataFunctionTests
{
    private readonly GetSiteMetaDataFunction _sut;
    private readonly Mock<ISiteService> _siteService = new();
    private readonly Mock<IValidator<SiteBasedResourceRequest>> _validator = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<ILogger<GetSiteMetaDataFunction>> _logger = new();

    public GetSiteMetaDataFunctionTests()
    {        
        _sut = new GetSiteMetaDataFunction(_siteService.Object, _validator.Object, _userContextProvider.Object, _logger.Object);
        _validator
            .Setup(x => x.ValidateAsync(It.IsAny<SiteBasedResourceRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    /*[Fact]
    public async Task RunAsync_ReturnsSuccess_WhenRequestedSiteIsConfigured()
    {
        _siteService.Setup(x => x.GetSiteByIdAsync("123")).ReturnsAsync(new Site("ABC01", "TEST", "Test", Enumerable.Empty<AttributeValue>(), new Location("", new[] { 0.0, 0.0 })));
        var request = CreateRequest();
        var result = await _sut.RunAsync(request) as ContentResult;
        result.StatusCode.Should().Be(200);
        var actualResponse = await ReadResponseAsync<GetSiteMetaDataResponse>(result.Content);
        actualResponse.Site.Should().Be("123");
        actualResponse.AdditionalInformation.Should().Be("Test data");
    }*/

    [Fact]
    public async Task RunAsync_ReturnsNotFound_WhenRequestedSiteIsNotConfigured()
    {
        var request = CreateRequest();
        var result = await _sut.RunAsync(request) as ContentResult;
        result.StatusCode.Should().Be(404);
    }

    private static HttpRequest CreateRequest()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.QueryString = new QueryString($"?site=123");
        request.Headers.Add("Authorization", "Test 123");
        return request;
    }

    private static async Task<TRequest> ReadResponseAsync<TRequest>(string response)
    {
        var body = await new StringReader(response).ReadToEndAsync();
        return JsonConvert.DeserializeObject<TRequest>(body);
    }
}
