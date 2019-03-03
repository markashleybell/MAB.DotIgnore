<Query Kind="Program">
  <Reference Relative="..\..\MAB.DotIgnore\bin\Release\net4\MAB.DotIgnore.dll">C:\Src\MAB.DotIgnore\MAB.DotIgnore\bin\Release\net4\MAB.DotIgnore.dll</Reference>
  <Namespace>MAB.DotIgnore</Namespace>
</Query>

void Main()
{
    var path = $@"{Path.GetDirectoryName(Util.CurrentQueryPath)}\..\..\profile\data";
    
    path.Dump();

    var fileList = TestData.GetFiles(path);
    var ignoreList = new IgnoreList(TestData.GetPatterns(path));
    
    var stopwatch = Stopwatch.StartNew();
    
    var result = fileList
        .Select(f => ignoreList.IsIgnored(f, pathIsDirectory: false))
        .ToArray();
    
    stopwatch.Stop();
    
    result.Count().Dump("Processed");
    
    stopwatch.ElapsedMilliseconds.Dump("Elapsed Time (ms)");
}

public static class TestData 
{
    public static string[] GetFiles(string path) =>
        ReadLines($@"{path}\filelist.txt")
            .Select(l => l.Trim('"').Replace(@"C:\", "").Replace(@"\", "/"))
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToArray();

    public static string[] GetPatterns(string path) =>
        ReadLines($@"{path}\.ignores").ToArray();

    public static IEnumerable<string> ReadLines(string path)
    {
        using (var sr = new StreamReader(File.OpenRead(path)))
        {
            var line = string.Empty;
            while ((line = sr.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }
}