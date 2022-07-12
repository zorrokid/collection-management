using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ImageUploader.StringUtils;

namespace ImageUploader.Uploader;

public interface IBlobStorageUploader
{
  void Upload(string folderPath, string containerName, CancellationToken cancellationToken);
}

public class BlobStorageUploader : IBlobStorageUploader
{
  private readonly string uploadPath;
  private readonly BlobServiceClient blobServiceClient;

  public BlobStorageUploader(string uploadPath, string storageAccountKey, string storageAccountName)
  {
    this.uploadPath = uploadPath;
    var sharedKeyCredential = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
    string blobUri = $"https://{storageAccountName}.blob.core.windows.net";
    blobServiceClient = new BlobServiceClient(new Uri(blobUri), sharedKeyCredential);
  }

  public async void Upload(string directoryPath, string containerName, CancellationToken cancellationToken = default)
  {
    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

    // Grouping files by product:
    // filename.1.jpg
    // filename.2.jpg
    // ...
    // filename.n.jpg

    var files = Directory.GetFiles(directoryPath);
    var groupedFiles = new StringGroup(files).GroupByPrefix(".");

    foreach (var key in groupedFiles.Keys)
    {
      foreach (var fileName in groupedFiles[key])
      {
        var blobClient = containerClient.GetBlobClient(fileName);
        var fileStream = File.OpenRead(fileName);
        var uploadOptions = new BlobUploadOptions
        {
          Tags = new Dictionary<string, string>
          {
            { "fileGroup", key }
          }
        };

        await blobClient.UploadAsync(fileStream, uploadOptions, cancellationToken);
      }
    }
  }
}
