namespace ImageUploader.StringUtils;

public interface IStringGroup
{
  Dictionary<string, List<string>> GroupByPrefix(string[] names, string separator);
}

public class StringGroup : IStringGroup
{
  public Dictionary<string, List<string>> GroupByPrefix(string[] names, string separator)
  {
    var groupedFileNames = new Dictionary<string, List<string>>();
    var filenameStack = new Stack<string>(names);
    while (filenameStack.Count > 0)
    {
      var fileName = filenameStack.Pop();
      var prefix = fileName.Split(".").First();
      if (groupedFileNames.ContainsKey(prefix) == false)
      {
        groupedFileNames[prefix] = new List<string>();
      }
      groupedFileNames[prefix].Add(fileName);
    }
    return groupedFileNames;
  }
}