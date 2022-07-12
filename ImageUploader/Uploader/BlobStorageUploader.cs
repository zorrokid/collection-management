using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ImageUploader.StringUtils;

namespace ImageUploader.Uploader;

public interface IBlobStorageUploader
{
  Task Upload(string directoryPath, string containerName, CancellationToken cancellationToken);
}

public class BlobStorageUploader : IBlobStorageUploader
{
  private readonly BlobServiceClient blobServiceClient;

  public BlobStorageUploader(string storageAccountKey, string storageAccountName)
  {
    var sharedKeyCredential = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
    string blobUri = $"https://{storageAccountName}.blob.core.windows.net";
    blobServiceClient = new BlobServiceClient(new Uri(blobUri), sharedKeyCredential);
  }

  public async Task Upload(string directoryPath, string containerName, CancellationToken cancellationToken = default)
  {
    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

    // Grouping files by product:
    // filename.1.jpg
    // filename.2.jpg
    // ...
    // filename.n.jpg

    var files = Directory.EnumerateFiles(directoryPath)
      .Select(f => Path.GetFileName(f));
    var groupedFiles = new StringGroup(files).GroupByPrefix(".");

    foreach (var key in groupedFiles.Keys)
    {
      foreach (var fileName in groupedFiles[key])
      {
        var blobClient = containerClient.GetBlobClient(fileName);
        var fileStream = File.OpenRead(Path.Combine(directoryPath, fileName));
        var uploadOptions = new BlobUploadOptions
        {
          Tags = new Dictionary<string, string>
          {
            { "fileGroup", key }
          }
        };
        Console.WriteLine($"Start uploading file {fileName} with fileGroup-tag {key}");
        await blobClient.UploadAsync(fileStream, uploadOptions, cancellationToken);
        Console.WriteLine($"Finish uploading file {fileName} with fileGroup-tag {key}");
      }
    }
  }
}
