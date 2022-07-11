using Azure.Storage;
using Azure.Storage.Blobs;
using ImageUploader.StringUtils;

namespace ImageUploader.Uploader;

public interface IBlobStorageUploader
{
  void Upload(string folderPath, string containerName);
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

  public void Upload(string directoryPath, string containerName)
  {
    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

    // Grouping files by product:
    // filename.1.jpg
    // filename.2.jpg
    // ...
    // filename.n.jpg

    var files = Directory.GetFiles(directoryPath);
    var group = new StringGroup();
    var groupedFiles = group.GroupByPrefix(files, ".");


    // var directoryInfo = new DirectoryInfo(directoryPath);
    // var files = directoryInfo.GetFileSystemInfos();
    // var orderedFiles = files.OrderBy(f => f.Name);
  }
}
