<Query Kind="Program">
  <Namespace>System.Runtime.InteropServices</Namespace>
</Query>

void Main()
{
    // https://github.com/git/git/blob/master/t/t3070-wildmatch.sh
    
    var workingDirectory = Path.GetDirectoryName(Util.CurrentQueryPath);

    var tests = File.ReadAllLines(workingDirectory + @"\tests.txt")
        .Where(l => !l.StartsWith("#") && !string.IsNullOrWhiteSpace(l))
        .Select(l => l.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
        .Select((l, i) => {
            var path = l[5].Trim('\'', '"');
            // This weirdness is because a few of the test patterns actually contain a space
            // character, so we tack the extra element created by the split onto the end
            var pattern = l[6].Trim('\'', '"') + (l.Length == 8 ? " " + l[7].Trim('\'', '"') : "");
        
            // Manually hack around the fact that test 82 passes in a space as the path, which makes 
            // both the pattern and path differ from the test file and causes the test to fail 
            if (i == 82)
            {
                path = " ";
                pattern = pattern.TrimStart(' ');
            }
        
            return new GitTest { 
                ID = i,
                Pattern = pattern,
                Path = path,
                ExpectGlobMatch = l[1] == "1",
                ExpectGlobMatchCI = l[2] == "1",
                ExpectPathMatch = l[3] == "1",
                ExpectPathMatchCI = l[4] == "1"
            };
        });
        
    // tests.Dump();
    
    // $"'{tests.Single(t => t.ID == 80).Path}'".Dump();
    
    var filter = new[] { 11, 12, 72, 73, 133, 134 };
    
    // tests = tests.Where(t => filter.Any(f => f == t.ID));
    
    var expected = tests.Select(t => new Check {
        ID = t.ID,
        Pattern = t.Pattern,
        Path = t.Path,
        Result = t.ExpectGlobMatch,
        ResultCI = t.ExpectGlobMatchCI
    });
    
    var actual = tests.Select(t => new Check {
        ID = t.ID,
        Pattern = t.Pattern,
        Path = t.Path,
        Regex = GetRegex(t.Pattern),
        Result = IsMatch(t.Pattern, t.Path),
        ResultCI = IsMatch(t.Pattern, t.Path, caseSensitive: false)
    });
    
    var failed = actual
        .Where(a => {
            var ex = expected.Single(e => e.ID == a.ID);
            return a.Result != ex.Result || a.ResultCI != ex.ResultCI;
        })
        .Select(a => new { 
            a.ID, 
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
    
    // Util.Dif(expected, actual).Dump();
    
    IsMatch(pattern: @"**[!te]", path: @"ten").Dump();
}

public static bool IsMatch(string pattern, string path, bool caseSensitive = true) =>
    TryMatch(GetRegex(pattern), path, caseSensitive);

public static bool TryMatch(string rxPattern, string path, bool caseSensitive = true)
{
    try
    {
        return !caseSensitive
            ? Regex.IsMatch(path, rxPattern, RegexOptions.IgnoreCase)
            : Regex.IsMatch(path, rxPattern);
    }
    catch
    {
        return false;
    }
}

public static string GetRegex(string pattern, bool caseSensitive = true)
{
    var literalsToEscapeInRegex = new[] { ".", "$", "{", "}", "(", "|", ")", "+" };

    // Match POSIX char classes with .NET unicode equivalents
    // https://www.regular-expressions.info/posixbrackets.html
    var charClassSubstitutions = new Dictionary<string, string> {
        { "[:alnum:]", @"a-zA-Z0-9" },
        { "[:alpha:]", @"a-zA-Z" },
        { "[:blank:]", @"\p{Zs}\t" },
        { "[:cntrl:]", @"\p{Cc}" },
        { "[:digit:]", @"\d" },
        { "[:graph:]", @"^\p{Z}\p{C}" },
        { "[:lower:]", @"a-z" },
        { "[:print:]", @"\p{C}" },
        { "[:punct:]", @"\p{P}" },
        { "[:space:]", @"\s" },
        { "[:upper:]", @"A-Z" },
        { "[:xdigit:]", @"A-Fa-f0-9" }
    };

    var charClasses = charClassSubstitutions.Keys.ToArray();

    var patternCharClasses = Regex.Matches(pattern, @"\[\:[a-z]+\:\]").Cast<Match>().Select(m => m.Groups[0].Value);

    if (patternCharClasses.Any(pcc => !charClasses.Any(cc => cc == pcc)))
    {
        // Malformed character class
        return null;
    }

    // Remove single backslashes before alphanumeric chars 
    // (escaping these in a glob pattern should have no effect)
    pattern = Regex.Replace(pattern, @"(?<!\\)\\([a-zA-Z0-9])", "$1");

    var rx = new StringBuilder(pattern);

    foreach(var literal in literalsToEscapeInRegex)
        rx.Replace(literal, @"\" + literal);

    foreach(var k in charClasses)
        rx.Replace(k, charClassSubstitutions[k]);

    rx.Replace("!", "^");
    rx.Replace("**", "[:GLOBSTAR:]");
    rx.Replace("*", "[:STAR:]");

    rx.Insert(0, "^");
    rx.Append("$");

    var rxs = rx.ToString();

    // Replace non-escaped star and question mark chars with equivalent regex patterns
    rxs = Regex.Replace(rxs, @"(?<!\\)\[\:STAR:\]", "[^/]*");
    rxs = Regex.Replace(rxs, @"\\\[\:STAR:\]", @"\*");
    rxs = Regex.Replace(rxs, @"(?<!\\)\?", "[^/]");
    rxs = Regex.Replace(rxs, @"(\[\:GLOBSTAR\:\]/?)+", ".*");

    return rxs;
}

public class GitTest
{
    public int ID { get; set; }
    public string Pattern { get; set; }
    public string Path { get; set; }
    public bool ExpectGlobMatch { get; set; }
    public bool ExpectGlobMatchCI { get; set; }
    public bool ExpectPathMatch { get; set; }
    public bool ExpectPathMatchCI { get; set; }
}

public class Check
{
    public int ID { get; set; }
    public string Pattern { get; set; }
    public string Path { get; set; }
    public string Regex { get; set; }
    public bool Result { get; set; }
    public bool ResultCI { get; set; }
}