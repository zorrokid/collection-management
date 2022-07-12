namespace ImageUploader.Uploader;

public interface IFileGroupTagResolver
{
  string Resolve(string fileName);
}

public class NamePrefixTagResolver : IFileGroupTagResolver
{
  private readonly string separator;

  public NamePrefixTagResolver(string separator = ".")
  {
    this.separator = separator;
  }

  public string Resolve(string fileName)
  {
    if (string.IsNullOrEmpty(fileName))
    {
      throw new Exception("Filename cannot be empty");
    }
    return fileName.Split(separator)[0];
  }
}

