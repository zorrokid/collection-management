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
  private readonly IFileGroupTagResolver tagResolver;

  public BlobStorageUploader(
    string storageAccountKey,
    string storageAccountName,
    IFileGroupTagResolver tagResolver,
    IBlobUriGenerator blobUriGenerator
  )
  {
    var sharedKeyCredential = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
    blobServiceClient = new BlobServiceClient(blobUriGenerator.Generate(storageAccountName), sharedKeyCredential);
    this.tagResolver = tagResolver;
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

      var uploadOptions = new BlobUploadOptions
      {
        Tags = new Dictionary<string, string>
          {
            { "fileGroup", tagResolver.Resolve(fileName) }
          }
      };
      Console.WriteLine($"Start uploading file {fileName}");
      await blobClient.UploadAsync(fileStream, uploadOptions, cancellationToken);
      Console.WriteLine($"Finish uploading file {fileName}");
    }
  }
}
