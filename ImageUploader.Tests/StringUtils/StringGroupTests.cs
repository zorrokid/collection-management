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

  [Fact]
  public void GroupByPrefix_StringsHaveOnlyPrefixParts_DictionaryWithEachStringInOwnList()
  {
    var group = new StringGroup();
    var input = new string[]
    {
      "aaa",
      "bbb",
      "ccc"
    };
    var result = group.GroupByPrefix(input, ".");
    Assert.NotEmpty(result);
    Assert.True(result.Keys.Count == 3, "Should have been keys for each string");
    Assert.NotNull(result["aaa"]);
    Assert.True(result["aaa"].Count == 1, "List should only have one item");
    Assert.True(result["aaa"][0] == "aaa", "List should contain key in this special case when string has only prefix part");
  }

  [Fact]
  public void GroupByPrefix_StringsWithEachSeparatePrefix_DictionaryWithEachStringInOwnList()
  {
    var group = new StringGroup();
    var input = new string[]
    {
      "aaa.1",
      "bbb.2",
      "ccc.3"
    };
    var result = group.GroupByPrefix(input, ".");
    Assert.NotEmpty(result);
    Assert.True(result.Keys.Count == 3, "Should have been keys for each string");
    Assert.NotNull(result["aaa"]);
    Assert.True(result["aaa"].Count == 1, "List should only have one item");
    Assert.True(result["aaa"][0] == "aaa.1");
  }

  [Fact]
  public void GroupByPrefix_StringsWithSamePredix_DictionaryPrefixKeyHavingListOfStrings()
  {
    var group = new StringGroup();
    var input = new string[]
    {
      "aaa.1",
      "aaa.2",
      "aaa.3"
    };
    var result = group.GroupByPrefix(input, ".");
    Assert.NotEmpty(result);
    Assert.True(result.Keys.Count == 1);
    Assert.NotNull(result["aaa"]);
    Assert.True(result["aaa"].Count == 3);
    Assert.True(result["aaa"].Contains("aaa.1"));
  }
}