using System.Linq;
using System.Text.RegularExpressions;

namespace CSharpx.TypeProviders.Regex
{
  public class RegexTypeProvider : TypeProvider
  {
    private readonly System.Text.RegularExpressions.Regex regex;
    private Match match;

    public RegexTypeProvider()
    {
    }

    public RegexTypeProvider(string pattern)
    {
      regex = new System.Text.RegularExpressions.Regex(pattern);
      Properties = regex.GetGroupNames().Where(name => name != "0").Select(name => new TypeProperty { Name = name, DataType = typeof(string) }).ToList();
    }

    public bool IsMatch(string text)
    {
      match = regex.Match(text);
      return match.Success;
    }

    public object GetPropertyValue(object name)
    {
      return match.Groups[name.ToString()];
    }

  }
}