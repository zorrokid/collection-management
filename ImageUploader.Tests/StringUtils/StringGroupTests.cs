namespace ImageUploader.Tests.StringUtils;
using ImageUploader.StringUtils;

public class StringGroupTests
{
  [Fact]
  public void GroupByPrefix_EmptyInputArray_EmptyResultDictionary()
  {
    var group = new StringGroup();
    var input = new string[0];
    var result = group.GroupByPrefix(input, ".");
    Assert.Empty(result);
  }
}