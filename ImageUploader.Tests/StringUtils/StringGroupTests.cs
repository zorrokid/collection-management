namespace ImageUploader.Tests.StringUtils;
using ImageUploader.StringUtils;

public class StringGroupTests
{
  [Fact]
  public void GroupByPrefix_EmptyInputArray_EmptyResultDictionary()
  {
    var input = new string[0];
    var group = new StringGroup(input);
    var result = group.GroupByPrefix(".");
    Assert.Empty(result);
  }

  [Fact]
  public void GroupByPrefix_StringsHaveOnlyPrefixParts_DictionaryWithEachStringInOwnList()
  {
    var input = new string[]
    {
      "aaa",
      "bbb",
      "ccc"
    };
    var group = new StringGroup(input);
    var result = group.GroupByPrefix(".");
    Assert.NotEmpty(result);
    Assert.True(result.Keys.Count == 3, "Should have been keys for each string, since they only have prefix parts.");
    Assert.NotNull(result["aaa"]);
    Assert.True(result["aaa"].Count == 1);
    Assert.True(result["aaa"][0] == "aaa", "List should contain same value as key in this special case when string has only prefix part.");
  }

  [Fact]
  public void GroupByPrefix_StringsWithEachSeparatePrefix_DictionaryWithEachStringInOwnList()
  {
    var input = new string[]
    {
      "aaa.1",
      "bbb.2",
      "ccc.3"
    };
    var group = new StringGroup(input);
    var result = group.GroupByPrefix(".");
    Assert.NotEmpty(result);
    Assert.True(result.Keys.Count == 3, "Should have been keys for each string since no duplicate prefixes.");
    Assert.NotNull(result["aaa"]);
    Assert.True(result["aaa"].Count == 1, "List should only have one item since no duplicate prefixes.");
    Assert.True(result["aaa"][0] == "aaa.1");
  }

  [Fact]
  public void GroupByPrefix_StringsWithSamePredix_DictionaryPrefixKeyHavingListOfStrings()
  {
    var input = new string[]
    {
      "aaa.1",
      "aaa.2",
      "aaa.3"
    };
    var group = new StringGroup(input);
    var result = group.GroupByPrefix(".");
    Assert.NotEmpty(result);
    Assert.True(result.Keys.Count == 1);
    Assert.NotNull(result["aaa"]);
    Assert.True(result["aaa"].Count == 3);
    Assert.True(result["aaa"].Contains("aaa.1"));
  }

  [Fact]
  public void GroupByPrefix_StringsWithMoreThanOneSeparator_ResultWithFirstPartAsKey()
  {
    var input = new string[]
    {
        "aaa.4.1",
        "aaa.5.2",
        "aaa.6.3"
    };
    var group = new StringGroup(input);
    var result = group.GroupByPrefix(".");
    Assert.NotEmpty(result);
    Assert.True(result.Keys.Count == 1);
    Assert.NotNull(result["aaa"]);
    Assert.True(result["aaa"].Count == 3);
    Assert.True(result["aaa"].Contains("aaa.4.1"));
  }
}