<Query Kind="Program">
  <Reference Relative="..\..\MAB.DotIgnore\bin\Release\netstandard1.3\MAB.DotIgnore.dll">E:\Src\MAB.DotIgnore\MAB.DotIgnore\bin\Release\netstandard1.3\MAB.DotIgnore.dll</Reference>
  <Namespace>MAB.DotIgnore</Namespace>
</Query>

#LINQPad optimize+

void Main()
{
    Util.NewProcess = true;
    
    var rx1 = new Regex(@"(\[:STARSTAR:\]/?)+", RegexOptions.Compiled);
    var rx2 = new Regex(@"(?>\[:STARSTAR:\]/?)+", RegexOptions.Compiled);
    
    var pattern = @"test/[:STARSTAR:]/[:STARSTAR:]/[:STARSTAR:]/[:STARSTAR:]/[:STARSTAR:]/[:STARSTAR:]/[:STARSTAR:]/[:STARSTAR:]";
    
    rx1.Replace(pattern, ".*").Dump("Match");
    rx2.Replace(pattern, ".*").Dump("Match");
    
    var ms1 = Benchmark.Perform(() => { var m = rx1.IsMatch(pattern); }, 10000000);
    var ms2 = Benchmark.Perform(() => { var m = rx2.IsMatch(pattern); }, 10000000);

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