using System;
using System.IO;
using System.Text;
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
    // Azure Function name and output Binding to Table Storage
    [FunctionName("ProcessImageUpload")]
    [return: Table("ImageText", Connection = "StorageConnection")]
    // Trigger binding runs when an image is uploaded to the blob container below
    public static async Task<ImageContent> Run(
      [BlobTrigger("azimgvizcontainer/{name}", Connection = "StorageConnection")] Stream myBlob,
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

      // Create Shared Access Signature 
      var sasUri = GetServiceSasUriForBlob(blobClient);

      var credentials = new ApiKeyServiceClientCredentials(subscriptionKey);
      var client = new ComputerVisionClient(credentials) { Endpoint = endpoint };
      // Get the analyzed image contents
      var textContext = await AnalyzeImageContent(client, sasUri.AbsoluteUri);
      log.LogInformation($"Image content analyzed: {textContext}");
      return new ImageContent { PartitionKey = "Images", RowKey = Guid.NewGuid().ToString(), Text = textContext };

    }
    public class ImageContent
    {
      public string PartitionKey { get; set; }
      public string RowKey { get; set; }
      public string Text { get; set; }
    }

    private static Uri GetServiceSasUriForBlob(BlobClient blobClient,
    string storedPolicyName = null)
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

        Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
        Console.WriteLine("SAS URI for blob is: {0}", sasUri);
        Console.WriteLine();

        return sasUri;
      }
      else
      {
        Console.WriteLine(@"BlobClient must be authorized with Shared Key 
                          credentials to create a service SAS.");
        return null;
      }
    }

    static async Task<string> AnalyzeImageContent(ComputerVisionClient client, string urlFile)
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
      while ((results.Status == OperationStatusCodes.Running ||
          results.Status == OperationStatusCodes.NotStarted));

      var textUrlFileResults = results.AnalyzeResult.ReadResults;

      // Assemble into readable string
      StringBuilder text = new StringBuilder();
      foreach (ReadResult page in textUrlFileResults)
      {
        foreach (Line line in page.Lines)
        {
          text.AppendLine(line.Text);
        }
      }

      return text.ToString();
    }
  }
}
