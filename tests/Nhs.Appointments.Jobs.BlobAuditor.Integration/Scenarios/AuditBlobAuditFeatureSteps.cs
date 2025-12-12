using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Jobs.BlobAuditor.Integration.Scenarios;

public class AuditBlobAuditFeatureSteps : BaseFeatureSteps
{
    protected override string ContainerName => "audit_data";
    private string TestPatitionKey => "integration_test";
    private string _lastAddFileId = "";
    
    [When("I add a file to container")]
    public async Task AddFile()
    {
        _lastAddFileId = Guid.NewGuid().ToString();
        await UploadFileToContainerAsync(new Dictionary<string, string>()
        {
            { "id", _lastAddFileId },
            { "user", TestPatitionKey },
            { "docType", "audit_test" },
        });
    }
    
    [And("I update that file")]
    public async Task UpdateFile()
    {
        await UploadFileToContainerAsync(new Dictionary<string, string>()
        {
            { "id", _lastAddFileId },
            { "user", TestPatitionKey },
            { "update", Guid.NewGuid().ToString() },
            { "docType", "audit_test" },
        });
    }
    
    [And("clean data")]
    public async Task CleanData()
    {
        var containerClient = GetContainer();
        foreach (var testFile in _testFiles)
        {
            await containerClient.DeleteItemAsync<object>(testFile["id"], new PartitionKey(testFile["user"]));
        }
    }

    [Then("files appear in blob")]
    public async Task AssertBlobsExists()
    {
        var containerClient = GetContainer();
        containerClient.Should().NotBeNull();
        
        foreach (var testFile in _testFiles)
        {
            var cosmosEntity = (await containerClient.ReadItemAsync<Dictionary<string, string>>(testFile["id"],
                new PartitionKey(testFile["user"]))).Resource;
            var blobContainer = GetBlobContainer();
            var blobClient = blobContainer.GetBlobClient(
                Path.Combine(Path.Combine(
                    $"{cosmosEntity["_ts"]:yyyyMMdd}",
                    cosmosEntity["docType"],
                    $"{cosmosEntity["_ts"]:yyyyMMddHHmmssfff}-{cosmosEntity["id"]}.json")));
            
            blobClient.Should().NotBeNull();

            await using var fileStream = await blobClient.OpenReadAsync();
            fileStream.Should().NotBeNull();
            
             
        }
    }
}
