﻿using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Functions;

public class GetSitesByAreaFunctionTests
{
    private readonly Mock<ISiteService> _siteService = new();
    private readonly Mock<IValidator<GetSitesByAreaRequest>> _validator = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<ILogger<GetSitesByAreaFunction>> _logger = new();
    private readonly GetSitesByAreaFunction _sut;

    public GetSitesByAreaFunctionTests()
    {
        _sut = new GetSitesByAreaFunction(_siteService.Object, _validator.Object, _userContextProvider.Object, _logger.Object);
        _validator
            .Setup(x => x.ValidateAsync(It.IsAny<GetSitesByAreaRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("attr,attr2,,attr3")]
    [InlineData("attr,attr2,attr3,")]
    [InlineData("")]
    public async Task RunAsync_ReturnsBadRequest_WhenAccessNeedsContainsEmptyString(string accessNeeds)
    {
        var request = CreateRequest(34.6, 2.1, 10, 10, true, accessNeeds);
        var result = await _sut.RunAsync(request) as ContentResult;
        result?.StatusCode.Should().Be(400);
        _siteService.Verify(x => x.FindSitesByArea(34.6, 2.1, 10, 10, Array.Empty<string>()), Times.Never());
    }
    
    [Fact]
    public async Task RunAsync_ReturnsSuccess_WhenSiteRequestedWithAccessNeeds()
    {
        const double longitude = 34.6;
        const double latitude = 2.1;
        const int searchRadius = 100000;
        const int maxRecords = 50;
        const string accessNeeds = "attr_1";
        var sites = new List<SiteWithDistance>()
        {
            new (
                new Site(
                    Id: "1", 
                    Name: "Alpha",
                    Address: "somewhere", 
                    AttributeValues: new [] {new AttributeValue(Id: "accessibility/attr_1", Value: "true")}, 
                    Location: new Location("point", [0.1, 10])),
                Distance: 100)
        };
        _siteService.Setup(x => x.FindSitesByArea(longitude, latitude, searchRadius, maxRecords, new[]{accessNeeds})).ReturnsAsync(sites);
        var request = CreateRequest(longitude, latitude, searchRadius, maxRecords, true, accessNeeds);
        var result = await _sut.RunAsync(request) as ContentResult;
        result?.StatusCode.Should().Be(200);
        _siteService.Verify(x => x.FindSitesByArea(longitude, latitude, searchRadius, maxRecords, new[]{accessNeeds}), Times.Once());
    }
    
    [Fact]
    public async Task RunAsync_ReturnsSuccess_WhenSitesRequestedWithoutAccessNeeds()
    {
        const double longitude = 34.6;
        const double latitude = 2.1;
        const int searchRadius = 50000;
        const int maxRecords = 50;
        var sites = new List<SiteWithDistance>()
        {
            new (
                new Site(
                    Id: "1", 
                    Name: "Alpha", 
                    Address: "somewhere", 
                    AttributeValues: new [] {new AttributeValue(Id: "accessibility/attr_1", Value: "true")}, 
                    Location: new Location("point", [0.1, 10])),
                Distance: 100)
        };
        _siteService.Setup(x => x.FindSitesByArea(longitude, latitude, searchRadius, maxRecords, Array.Empty<string>())).ReturnsAsync(sites);
        var request = CreateRequest(longitude, latitude, searchRadius, maxRecords, false);
        var result = await _sut.RunAsync(request) as ContentResult;
        result?.StatusCode.Should().Be(200);
        _siteService.Verify(x => x.FindSitesByArea(longitude, latitude, searchRadius, maxRecords, Array.Empty<string>()), Times.Once());
    }


    private static HttpRequest CreateRequest(double longitude, double latitude, int searchRadius, int maxRecords, bool includeAccessNeedsWhenEmpty, params string[] accessNeeds)
    {
        var context = new DefaultHttpContext();
        var queryString = $"?long={longitude}&lat={latitude}&searchRadius={searchRadius}&maxRecords={maxRecords}";
        if (accessNeeds.Any() || includeAccessNeedsWhenEmpty)
            queryString += $"&accessNeeds={string.Join(",", accessNeeds)}";
        var request = context.Request;
        request.QueryString = new QueryString(queryString);
        return request;
    }
    
    private static async Task<TRequest?> ReadResponseAsync<TRequest>(string response)
    {
        var body = await new StringReader(response).ReadToEndAsync();
        return JsonConvert.DeserializeObject<TRequest>(body);
    }
}