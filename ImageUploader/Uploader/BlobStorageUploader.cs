using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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
    var files = Directory.EnumerateFiles(directoryPath)
      .Select(f => Path.GetFileName(f));

    foreach (var fileName in files)
    {
      var blobClient = containerClient.GetBlobClient(fileName);
      var fileStream = File.OpenRead(Path.Combine(directoryPath, fileName));

      // Grouping files by file prefix:
      // filename.1.jpg
      // filename.2.jpg
      // ...
      // filename.n.jpg
      var fileGroupKey = fileName.Split(".")[0];
      var uploadOptions = new BlobUploadOptions
      {
        Tags = new Dictionary<string, string>
          {
            { "fileGroup", fileGroupKey }
          }
      };
      Console.WriteLine($"Start uploading file {fileName} with fileGroup-tag {fileGroupKey}");
      await blobClient.UploadAsync(fileStream, uploadOptions, cancellationToken);
      Console.WriteLine($"Finish uploading file {fileName} with fileGroup-tag {fileGroupKey}");
    }
  }
}
