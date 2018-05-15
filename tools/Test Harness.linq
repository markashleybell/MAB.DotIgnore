<Query Kind="Program">
  <Reference Relative="..\MAB.DotIgnore\bin\Debug\netstandard1.3\MAB.DotIgnore.dll">E:\Src\MAB.DotIgnore\MAB.DotIgnore\bin\Debug\netstandard1.3\MAB.DotIgnore.dll</Reference>
  <Namespace>MAB.DotIgnore</Namespace>
</Query>

void Main()
{
    var paths = new List<TestPath> {
        new TestPath { Path = "one", IsDirectory = true },
        new TestPath { Path = "one/two", IsDirectory = true },
        new TestPath { Path = "two", IsDirectory = true }
    };

    var ignorePatterns = new string[] {
        "one/",
        "two/",
        "!one/two/"
    };
    
    var log = new IgnoreLog();
    
    var ignoreList = new IgnoreList(ignorePatterns, MatchFlags.PATHNAME | MatchFlags.CASEFOLD);
    paths.ForEach(path => ignoreList.IsIgnored(path.Path, path.IsDirectory, log).Dump("Ignore '" + path.Path + "'"));
    
//	var ignoreRule = new IgnoreRule(ignorePatterns[0], MatchFlags.PATHNAME | MatchFlags.CASEFOLD);
//  paths.ForEach(path => ignoreRule.IsMatch(path.Path, path.IsDirectory).Dump("Ignore '" + path.Path + "'"));
    
    log.Dump();
    log.ToString().Dump();
}

public class TestPath
{
    public string Path { get; set; }
    public bool IsDirectory { get; set; }
}