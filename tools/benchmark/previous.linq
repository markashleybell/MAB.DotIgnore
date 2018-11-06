<Query Kind="Program">
  <Reference Relative="previous\MAB.DotIgnore.dll">C:\Src\MAB.DotIgnore\tools\benchmark\previous\MAB.DotIgnore.dll</Reference>
  <Namespace>MAB.DotIgnore</Namespace>
</Query>

#LINQPad optimize+

void Main()
{
    var workingDirectory = Path.GetDirectoryName(Util.CurrentQueryPath);
    
    // This gives us an array of ~1500 file paths
    var fileList = File.ReadAllLines($@"{workingDirectory}\filelist.txt")
        .Select(l => l.Trim('"').Replace(@"C:\", "").Replace(@"\", "/"))
        .Where(l => !string.IsNullOrWhiteSpace(l))
        .ToList();

    var ignoreList = new IgnoreList($@"{workingDirectory}\.ignores");

    Action action = () => fileList.ForEach(f => ignoreList.IsIgnored(f, pathIsDirectory: false));

    var ms = Benchmark.Perform(action, 50);
    
    $"Completed in {ms}ms".Dump("Result");
}

public class Benchmark
{
	public static long Perform(Action action, int iterations = 1)
	{
		var stopwatch = Stopwatch.StartNew();
        
		for (int i = 0; i < iterations; i++)
		{
			action();
		}
        
		stopwatch.Stop();
        
		return stopwatch.ElapsedMilliseconds;
	}
}