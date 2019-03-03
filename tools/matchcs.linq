<Query Kind="Program">
  <Reference Relative="..\MAB.DotIgnore\bin\Debug\netstandard1.3\MAB.DotIgnore.dll">C:\Src\MAB.DotIgnore\MAB.DotIgnore\bin\Debug\netstandard1.3\MAB.DotIgnore.dll</Reference>
  <Namespace>MAB.DotIgnore</Namespace>
  <Namespace>System.Runtime.InteropServices</Namespace>
</Query>

void Main()
{    
    var workingDirectory = Path.GetDirectoryName(Util.CurrentQueryPath);

    // https://github.com/git/git/blob/master/t/t3070-wildmatch.sh
    var testLineRx = new Regex(@"^match ([01]) ([01]) ([01]) ([01]) ('.+?'|.+?) ('.+?'|.+?)$", RegexOptions.IgnoreCase);

    var tests = File.ReadAllLines(workingDirectory + @"\..\MAB.DotIgnore.Test\test_content\git-tests\tests-current-fixed.txt")
        .Select((s, i) => (content: s, number: i))
        .Where(line => !line.content.StartsWith("#", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(line.content))
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
        
    // tests.Dump();
    
    // $"'{tests.Single(t => t.ID == 80).Path}'".Dump();
    
    var filter = new[] { 11, 12, 72, 73, 133, 134 };
    
    // tests = tests.Where(t => filter.Any(f => f == t.ID));
    
    var expected = tests.Select(t => {
        var pattern = TrimQuotes(t.Pattern);
        var path = TrimQuotes(t.Path);
        
        return new MatchTestResult {
            LineNumber = t.LineNumber,
            Pattern = pattern,
            Path = path,
            Regex = Matcher.ToRegex(pattern),
            Result = t.ExpectGlobMatch,
            ResultCI = t.ExpectGlobMatchCI
        };
    });
    
    var actual = tests.Select(t => {
        var pattern = TrimQuotes(t.Pattern);
        var path = TrimQuotes(t.Path);
        
        var rxPattern = Matcher.ToRegex(pattern);

        Regex rx = null;
        Regex rxCI = null;
        
        try
        {
            rx = new Regex(rxPattern);
            rxCI = new Regex(rxPattern, RegexOptions.IgnoreCase);
        }
        catch {}

        return new MatchTestResult {
            LineNumber = t.LineNumber,
            Pattern = pattern,
            Path = path,
            Regex = rxPattern,
            Result = Matcher.TryMatch(rx, path),
            ResultCI = Matcher.TryMatch(rxCI, path)
        };
    });
    
    var failed = actual
        .Where(a => {
            var ex = expected.Single(e => e.LineNumber == a.LineNumber);
            return a.Result != ex.Result || a.ResultCI != ex.ResultCI;
        })
        .Select(a => new { 
            a.LineNumber, 
            a.Pattern, 
            a.Path, 
            Regex = a.Regex,
            Expected = !a.Result, 
            Actual = a.Result, 
            ExpectedCI = !a.ResultCI,
            ActualCI = a.ResultCI
        });
    
    // expected.Dump();
    // actual.Dump();
    
    failed.Dump();
    
    // failed.Select(f => f.Pattern).Dump();
    
    Util.Dif(expected, actual).Dump();
}

public static string TrimQuotes(string s) => s.Trim('\'', '"');

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

public class MatchTestResult
{
    public int LineNumber { get; set; }
    public string Pattern { get; set; }
    public string Path { get; set; }
    public string Regex { get; set; }
    public bool Result { get; set; }
    public bool ResultCI { get; set; }
}