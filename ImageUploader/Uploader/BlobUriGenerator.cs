namespace ImageUploader.Uploader;

public interface IBlobUriGenerator
{
  Uri Generate(string storageAccountName);
}

public class BlobUriGenerator : IBlobUriGenerator
{
  public Uri Generate(string storageAccountName)
  {
    return new Uri($"https://{storageAccountName}.blob.core.windows.net");
  }
}