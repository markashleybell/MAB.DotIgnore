<Query Kind="Program">
  <Reference Relative="..\..\MAB.DotIgnore\bin\Debug\netstandard1.3\MAB.DotIgnore.dll">E:\Src\MAB.DotIgnore\MAB.DotIgnore\bin\Debug\netstandard1.3\MAB.DotIgnore.dll</Reference>
  <Namespace>MAB.DotIgnore</Namespace>
  <Namespace>System.Runtime.InteropServices</Namespace>
</Query>

void Main()
{
    // https://github.com/git/git/blob/master/t/t3070-wildmatch.sh
    
    var workingDirectory = Path.GetDirectoryName(Util.CurrentQueryPath);

    var tests = File.ReadAllLines(workingDirectory + @"\tests.txt")
        .Where(l => !l.StartsWith("#") && !string.IsNullOrWhiteSpace(l))
        .Select(l => l.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
        .Select(l => new Test { 
            Path = l[5].Trim('\''),
            Pattern = l[6].Trim('\''),
            ExpectGlobMatch = l[1] == "1",
            ExpectGlobMatchCI = l[2] == "1",
            ExpectPathMatch = l[3] == "1",
            ExpectPathMatchCI = l[4] == "1"
        })
        .ToList();
        
    tests.Dump();
}

public class Test
{
    public string Path { get; set; }
    public string Pattern { get; set; }
    public bool ExpectGlobMatch { get; set; }
    public bool ExpectGlobMatchCI { get; set; }
    public bool ExpectPathMatch { get; set; }
    public bool ExpectPathMatchCI { get; set; }
}
