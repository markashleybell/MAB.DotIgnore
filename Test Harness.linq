<Query Kind="Program">
  <Reference Relative="MAB.DotIgnore\bin\Debug\MAB.DotIgnore.dll">E:\Src\MAB.DotIgnore\MAB.DotIgnore\bin\Debug\MAB.DotIgnore.dll</Reference>
  <Namespace>MAB.DotIgnore</Namespace>
</Query>

void Main()
{
    var paths = new List<TestPath> {
        new TestPath { Path = "ignored", IsDirectory = true },
        new TestPath { Path = "notignored", IsDirectory = true }
    };

    var ignorePatterns = new string[] {
        "ignored/"
    };
    
    var ignoreList = new IgnoreList(ignorePatterns, MatchFlags.PATHNAME | MatchFlags.CASEFOLD);
    paths.ForEach(path => ignoreList.IsIgnored(path.Path, path.IsDirectory).Dump("Ignore '" + path.Path + "'"));
    
    paths.Where(p => !p.IsDirectory).ToList().ForEach(path => ignoreList.IsAncestorIgnored(path.Path).Dump("Ignore '" + path.Path + "' Ancestor"));
    
	var ignoreRule = new IgnoreRule(ignorePatterns[0], MatchFlags.PATHNAME | MatchFlags.CASEFOLD);
    paths.ForEach(path => ignoreRule.IsMatch(path.Path, path.IsDirectory).Dump("Ignore '" + path.Path + "'"));
}

public class TestPath
{
    public string Path { get; set; }
    public bool IsDirectory { get; set; }
}