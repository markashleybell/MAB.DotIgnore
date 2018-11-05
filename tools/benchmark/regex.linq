<Query Kind="Program">
  <Reference Relative="..\..\MAB.DotIgnore\bin\Release\netstandard1.3\MAB.DotIgnore.dll">C:\Src\MAB.DotIgnore\MAB.DotIgnore\bin\Release\netstandard1.3\MAB.DotIgnore.dll</Reference>
  <Namespace>MAB.DotIgnore</Namespace>
</Query>

#LINQPad optimize+

void Main()
{
    var rx = new Regex(@"\[\:[a-z]*\:\]", RegexOptions.Compiled);
    var pattern = "[[:alnum:][:alpha:][:blank:][:cntrl:][:digit:][:graph:][:lower:][:print:][:punct:][:space:][:upper:][:xdigit:]]";
    
    var ms = Benchmark.Perform(() => rx.Matches(pattern).Cast<Match>().Select(m => m.Groups[0].Value), 1000000);
    
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