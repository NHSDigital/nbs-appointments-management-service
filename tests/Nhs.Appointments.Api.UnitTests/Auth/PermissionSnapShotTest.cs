﻿using FluentAssertions;
using Nhs.Appointments.Api.Auth;
using System.Reflection;
using Microsoft.Azure.Functions.Worker;
using Nhs.Appointments.Api.Functions;

namespace Nhs.Appointments.Api.Tests.Auth;

public class PermissionSnapShotTest
{
    [Theory]
    [InlineData("post", "booking/{bookingReference}/cancel", "booking:cancel")]
    [InlineData("get", "sites/{site}/meta", "site:get-meta-data")]
    [InlineData("post", "booking", "booking:make")]
    [InlineData("post", "booking/{bookingReference}/confirm", "booking:make")]
    [InlineData("post", "availability/query", "availability:query")]
    [InlineData("get", "booking", "booking:query")]
    [InlineData("get", "booking/{bookingReference}", "booking:query")]
    [InlineData("post", "booking/query", "booking:query")]
    [InlineData("post", "booking/set-status", "booking:set-status")]
    [InlineData("post", "user/roles", "users:manage")]
    [InlineData("get", "users", "users:view")]
    [InlineData("get", "sites", "sites:query")]
    [InlineData("get", "sites/{site}", "site:view")]
    [InlineData("post", "sites/{site}/accessibilities", "site:manage")]
    [InlineData("post", "availability/apply-template", "availability:setup")]
    [InlineData("post", "availability", "availability:setup")]
    [InlineData("post", "user/remove", "users:manage")]
    [InlineData("post", "system/run-reminders", "system:run-reminders")]
    [InlineData("post", "system/run-provisional-sweep", "system:run-provisional-sweep")]
    public void Permissions_AreApplied_ToCorrectEndpoints(string method, string path, string permission)
    {
        var functionTypes = typeof(BaseApiFunction<,>).Assembly.GetTypes().Where(t => t.GetMethod("RunAsync") != null);
        var entryPoints = functionTypes.Select(t => t.GetMethod("RunAsync"));
        var entryPointInfo = entryPoints.Select(
            ep => new
            {
                Trigger = ep.GetParameters().First().GetCustomAttribute<HttpTriggerAttribute>(),
                Permission = ep.GetCustomAttribute<RequiresPermissionAttribute>()?.Permission ?? ""
            }).Where(ep => ep.Trigger != null);

        var targetEntryPoint = entryPointInfo.SingleOrDefault(ep => ep.Trigger.Methods.Contains(method) && ep.Trigger.Route! == path);
        targetEntryPoint.Should().NotBeNull();
        targetEntryPoint.Permission.Should().Be(permission);
    }
}
