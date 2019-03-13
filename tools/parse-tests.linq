<Query Kind="Program">
  <Namespace>System.Runtime.InteropServices</Namespace>
</Query>

void Main()
{    
    var workingDirectory = Path.GetDirectoryName(Util.CurrentQueryPath);

    var testLineRx = new Regex(@"^match ([01]) ([01]) ([01]) ([01]) ('.+?'|.+?) ('.+?'|.+?)$", RegexOptions.IgnoreCase);

    var tests = File.ReadAllLines(workingDirectory + @"\..\MAB.DotIgnore.Test\test_content\git-tests\tests-current-fixed.txt")
        .Select((s, i) => (content: s, number: i))
        .Where(line => !line.content.StartsWith("#") && !string.IsNullOrWhiteSpace(line.content))
        .Select(line => (match: testLineRx.Match(line.content), lineNo: line.number))
        .Select(test =>  new GitTest { 
            LineNumber = test.lineNo,
            Pattern = test.match.Groups[6].Value,
            Path = test.match.Groups[5].Value,
            ExpectGlobMatch = test.match.Groups[1].Value == "1",
            ExpectGlobMatchCI = test.match.Groups[2].Value == "1",
            ExpectPathMatch = test.match.Groups[3].Value == "1",
            ExpectPathMatchCI = test.match.Groups[4].Value == "1"
        });
        
    tests.Dump();
}

public class GitTest
{
    public int LineNumber { get; set; }
    public string Pattern { get; set; }
    public string Path { get; set; }
    public bool ExpectGlobMatch { get; set; }
    public bool ExpectGlobMatchCI { get; set; }
    public bool ExpectPathMatch { get; set; }
    public bool ExpectPathMatchCI { get; set; }
}
