using ImageUploader.Uploader;

namespace ImageUploader.Tests.Uploader;

public class NamePrefixTagResolverTests
{
  public void Resolve_EmptyFileName_ThrowsException()
  {
    var resolver = new NamePrefixTagResolver();
    Assert.Throws<Exception>(() => resolver.Resolve(""));
  }

  public void Resolve_FileNameWithOnlyPrefix_ReturnsFileName()
  {
    var expected = "test";
    var resolver = new NamePrefixTagResolver();
    var result = resolver.Resolve(expected);
    Assert.Equal(expected, result);
  }

  public void Resolve_FileNameWithPrefixAndPostFix_ReturnsPrefix()
  {
    var expectedPrefix = "test";
    var resolver = new NamePrefixTagResolver();
    var result = resolver.Resolve($"{expectedPrefix}.1.jpg");
    Assert.Equal(expectedPrefix, result);
  }
}