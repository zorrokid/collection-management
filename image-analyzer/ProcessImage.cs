using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace ProcessImage
{
    public class ProcessImage
    {
        [FunctionName("ProcessImageUpload")]
        public static async Task Run(
          [BlobTrigger("azimgvizcontainer/{name}", Connection = "StorageConnection")] Stream myBlob,
          [CosmosDB(
                databaseName: "azimgviz-ocr-results-db",
                collectionName: "ocr-results",
                ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<dynamic> documentsOut,
          string name, ILogger log)
        {
            log.LogInformation($"Function ProcessImageUpload triggered with name: {name}");
            // Get connection configurations
            string subscriptionKey = Environment.GetEnvironmentVariable("ComputerVisionKey");
            string endpoint = Environment.GetEnvironmentVariable("ComputerVisionEndpoint");
            string accountName = Environment.GetEnvironmentVariable("StorageAccountName");
            string storageConnection = Environment.GetEnvironmentVariable("StorageConnection");
            string imgUrl = $"https://{accountName}.blob.core.windows.net/azimgvizcontainer/{name}";

            var blobServiceClient = new BlobServiceClient(storageConnection);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient("azimgvizcontainer");
            var blobClient = blobContainerClient.GetBlobClient(name);
            var blobPropertiesResult = await blobClient.GetPropertiesAsync();

            var blobProperties = blobPropertiesResult.Value;
            var fileGroupTag = blobProperties.Metadata["fileGroup"];

            log.LogInformation($"Got fileGroupTag {fileGroupTag}");

            // Create Shared Access Signature 
            var sasUri = GetServiceSasUriForBlob(blobClient);

            var credentials = new ApiKeyServiceClientCredentials(subscriptionKey);
            var client = new ComputerVisionClient(credentials) { Endpoint = endpoint };
            var readResults = await AnalyzeImageContent(client, sasUri.AbsoluteUri);

            await documentsOut.AddAsync(new
            {
                id = System.Guid.NewGuid().ToString(),
                sourceId = fileGroupTag,
                readResults
            });
        }

        private static Uri GetServiceSasUriForBlob(BlobClient blobClient, string storedPolicyName = null)
        {
            // Check whether this BlobClient object has been authorized with Shared Key.
            if (blobClient.CanGenerateSasUri)
            {
                // Create a SAS token that's valid for one hour.
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                    BlobName = blobClient.Name,
                    Resource = "b"
                };

                if (storedPolicyName == null)
                {
                    sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
                    sasBuilder.SetPermissions(BlobSasPermissions.Read |
                        BlobSasPermissions.Write);
                }
                else
                {
                    sasBuilder.Identifier = storedPolicyName;
                }

                return blobClient.GenerateSasUri(sasBuilder);
            }
            else
            {
                Console.WriteLine(@"BlobClient must be authorized with Shared Key credentials to create a service SAS.");
                return null;
            }
        }

        static async Task<IList<ReadResult>> AnalyzeImageContent(ComputerVisionClient client, string urlFile)
        {
            // Analyze the file using Computer Vision Client
            var textHeaders = await client.ReadAsync(urlFile);
            string operationLocation = textHeaders.OperationLocation;
            Thread.Sleep(2000);

            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            // Read back the results from the analysis request
            ReadOperationResult results;
            do
            {
                results = await client.GetReadResultAsync(Guid.Parse(operationId));
            }
            while (results.Status == OperationStatusCodes.Running ||
                results.Status == OperationStatusCodes.NotStarted);

            return results.AnalyzeResult.ReadResults;
        }
    }
}
