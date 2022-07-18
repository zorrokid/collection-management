using System;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;

namespace ProcessImage.Services;

public interface IBlobStorageSasUriProvider
{
    Uri GetServiceSasUriForBlob(string storedPolicyName = null);
}

public class BlobStorageSasUriProvider : IBlobStorageSasUriProvider
{
    private readonly BlobClient blobClient;
    private readonly ILogger log;

    public BlobStorageSasUriProvider(BlobClient blobClient, ILogger log)
    {
        this.blobClient = blobClient;
        this.log = log;
    }

    public Uri GetServiceSasUriForBlob(string storedPolicyName = null)
    {
        // Check whether this BlobClient object has been authorized with Shared Key.
        if (!blobClient.CanGenerateSasUri)
        {
            log.LogError("BlobClient must be authorized with Shared Key credentials to create a service SAS.");
            return null;
        }

        // Create a SAS token that's valid for one hour.
        var sasBuilder = new BlobSasBuilder()
        {
            BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
            BlobName = blobClient.Name,
            Resource = "b"
        };

        if (storedPolicyName == null)
        {
            sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
            sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write);
        }
        else
        {
            sasBuilder.Identifier = storedPolicyName;
        }

        return blobClient.GenerateSasUri(sasBuilder);
    }
}