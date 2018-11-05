<Query Kind="Program">
  <Namespace>System.Runtime.InteropServices</Namespace>
</Query>

void Main()
{    
    var workingDirectory = Path.GetDirectoryName(Util.CurrentQueryPath);

    // https://github.com/git/git/blob/master/t/t3070-wildmatch.sh
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
        
    // tests.Dump();
    
    // $"'{tests.Single(t => t.ID == 80).Path}'".Dump();
    
    var filter = new[] { 11, 12, 72, 73, 133, 134 };
    
    // tests = tests.Where(t => filter.Any(f => f == t.ID));
    
    var expected = tests.Select(t => {
        var pattern = TrimQuotes(t.Pattern);
        var path = TrimQuotes(t.Path);
        
        return new Check {
            LineNumber = t.LineNumber,
            Pattern = pattern,
            Path = path,
            Regex = GetRegex(pattern),
            Result = t.ExpectGlobMatch,
            ResultCI = t.ExpectGlobMatchCI
        };
    });
    
    var actual = tests.Select(t => {
        var pattern = TrimQuotes(t.Pattern);
        var path = TrimQuotes(t.Path);
        
        return new Check {
            LineNumber = t.LineNumber,
            Pattern = pattern,
            Path = path,
            Regex = GetRegex(pattern),
            Result = IsMatch(pattern, path),
            ResultCI = IsMatch(pattern, path, caseSensitive: false)
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
    
    // IsMatch(pattern: @"test[^A-Z]end", path: @"test/end").Dump();
    // IsMatch(pattern: @"test[^A-Z]end", path: @"test1end").Dump();
}

public static string TrimQuotes(string s) =>
    s.Trim('\'', '"');

public static bool IsMatch(string pattern, string path, bool caseSensitive = true) =>
    TryMatch(GetRegex(pattern), path, caseSensitive);

public static bool TryMatch(string rxPattern, string path, bool caseSensitive = true)
{
    if (string.IsNullOrEmpty(rxPattern))
    {
        return false;
    }
    
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

// https://git-scm.com/docs/gitignore#_pattern_format

// FNM_PATHNAME
// If this flag is set, match a slash in string only with a slash in pattern 
// and not by an asterisk (*) or a question mark (?) metacharacter, nor by a 
// bracket expression ([]) containing a slash.

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

    var patternCharClasses = Regex.Matches(pattern, @"\[\:[a-z]*\:\]").Cast<Match>().Select(m => m.Groups[0].Value);

    if (patternCharClasses.Any(pcc => !charClasses.Any(cc => cc == pcc)))
    {
        // Malformed character class
        return null;
    }
    
    // Double-star is only valid:
    // - at the beginning of a pattern, immediately followed by a slash ('**/c')
    // - at the end of a pattern, immediately preceded by a slash ('a/**')
    // - anywhere in the pattern with a slash immediately before and after ('a/**/c')
    if (Regex.IsMatch(pattern, @"\*\*[^/\s]|[^/\s]\*\*"))
    {
        return null;
    }

    // Remove single backslashes before alphanumeric chars 
    // (escaping these in a glob pattern should have no effect)
    pattern = Regex.Replace(pattern, @"(?<!\\)\\([a-zA-Z0-9])", "$1");

    var rx = new StringBuilder(pattern);

    foreach(var literal in literalsToEscapeInRegex)
    {
        rx.Replace(literal, @"\" + literal);
    }

    foreach(var k in charClasses)
    {
        rx.Replace(k, charClassSubstitutions[k]);
    }

    rx.Replace("!", "^");
    rx.Replace("**", "[:STARSTAR:]");
    rx.Replace("*", "[:STAR:]");
    rx.Replace("?", "[:QM:]");

    rx.Insert(0, "^");
    rx.Append("$");

    var rxs = rx.ToString();

    // Character class patterns shouldn't match slashes, so we prefix them with 
    // negative lookaheads. This is rather harder than it seems, because class 
    // patterns can also contain unescaped square brackets...
    
    // TODO: is this only true if PATHMATCH isn't specified?
    rxs = NonPathMatchCharClasses(rxs);
    
    // Non-escaped question mark should match any single char except slash
    rxs = Regex.Replace(rxs, @"\\\[:QM:\]", @"\?");
    rxs = Regex.Replace(rxs, @"(?<!\\)\[\:QM\:\]", "[^/]");
    
    // Replace star patterns with equivalent regex patterns
    rxs = Regex.Replace(rxs, @"(\[\:STARSTAR\:\]/?)+", ".*");
    rxs = Regex.Replace(rxs, @"\\\[\:STAR:\]", @"\*");
    rxs = Regex.Replace(rxs, @"(?<!\\)\[\:STAR:\]", "[^/]*");

    return rxs;
}

public static string NonPathMatchCharClasses(string p)
{
    var o = new StringBuilder();
    var inBrackets = false;
    
    for (var i = 0; i < p.Length;)
    {
        var escaped = i != 0 && p[i - 1] == '\\';
        
        if (p[i] == '[' && !escaped)
        {
            if (inBrackets)
            {
                o.Append(@"\");    
            }
            else
            {
                if (i < p.Length && p[i + 1] != ':')
                {
                    o.Append(@"(?!/)");
                }
                
                inBrackets = true;
            }
        }
        else if (p[i] == ']' && !escaped)
        {
            if (inBrackets && p.IndexOf(']', i + 1) >= p.IndexOf('[', i + 1))
            {
                inBrackets = false;
            }
        }

        o.Append(p[i++]);
    }
    
    return o.ToString();
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

public class Check
{
    public int LineNumber { get; set; }
    public string Pattern { get; set; }
    public string Path { get; set; }
    public string Regex { get; set; }
    public bool Result { get; set; }
    public bool ResultCI { get; set; }
}