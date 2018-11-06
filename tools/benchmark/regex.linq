<Query Kind="Program">
  <Reference Relative="..\..\MAB.DotIgnore\bin\Release\netstandard1.3\MAB.DotIgnore.dll">C:\Src\MAB.DotIgnore\MAB.DotIgnore\bin\Release\netstandard1.3\MAB.DotIgnore.dll</Reference>
  <Namespace>MAB.DotIgnore</Namespace>
</Query>

#LINQPad optimize+

void Main()
{
    var rx1 = new Regex(@"\[:[a-z]*?:\]", RegexOptions.Compiled);
    var rx2 = new Regex(@"\[:(?>[a-z]*?):\]", RegexOptions.Compiled);
    
    var pattern = "[[:alnum:][:alpha:][:blank:][:cntrl:][:digit:][:graph:][:lower:][:print:][:punct:][:space:][:upper:][:xdigit:]]";
    
    var ms1 = Benchmark.Perform(() => rx1.Matches(pattern), 10000000);
    var ms2 = Benchmark.Perform(() => rx2.Matches(pattern), 10000000);

    $"RX 1 completed in {ms1}ms".Dump("Result");
    $"RX 2 completed in {ms2}ms".Dump("Result");
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