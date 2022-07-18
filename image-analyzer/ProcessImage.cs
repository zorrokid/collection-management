using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using ProcessImage.Services;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
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
            var sasUriProvider = new BlobStorageSasUriProvider(blobClient, log);
            var sasUri = sasUriProvider.GetServiceSasUriForBlob();

            // Create ComputerVisionClient and analyzer image content
            var credentials = new ApiKeyServiceClientCredentials(subscriptionKey);
            var client = new ComputerVisionClient(credentials) { Endpoint = endpoint };
            var imageAnalyzer = new ImageAnalyzer(client);
            var readResults = await imageAnalyzer.AnalyzeImageContent(sasUri.AbsoluteUri);

            await documentsOut.AddAsync(new
            {
                id = System.Guid.NewGuid().ToString(),
                sourceId = fileGroupTag,
                readResults
            });
        }
    }
}
