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
        
            // Manually hack around the fact that test 80 passes in a space as the path, which makes 
            // both the pattern and path differ from the test file and causes the test to fail 
            if (i == 80)
            {
                path = " ";
                pattern = pattern.TrimStart(' ');
            }
        
            return new Test { 
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
        Result = t.ExpectPathMatch,
        ResultCI = t.ExpectPathMatchCI
    });
    
    var actual = tests.Select(t => new Check {
        ID = t.ID,
        Pattern = t.Pattern,
        Path = t.Path,
        Result = Match(t.Pattern, t.Path),
        ResultCI = Match(t.Pattern, t.Path, caseSensitive: false)
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
            Expected = !a.Result, 
            Actual = a.Result, 
            ExpectedCI = !a.ResultCI,
            ActualCI = a.ResultCI
        });
    
    // expected.Dump();
    
    failed.Dump();
    
    Util.Dif(expected, actual).Dump();
    
    // Match(pattern: @"\??\?b", path: @"?a?b", dumpRegex: true).Dump();
    // Match(pattern: @"XXX/*/*/*/*/*/*/12/*/*/*/m/*/*/*", path: @"XXX/adobe/courier/bold/o/normal//12/120/75/75/m/70/iso8859/1", dumpRegex: true).Dump();
}

public bool Match(string pattern, string path, bool caseSensitive = true)
{
    var literalsToEscapeInRegex = new[] { ".", "$", "{", "}", "(", "|", ")", "+" };
    
    var charClassSubstitutions = new Dictionary<string, string> {
        { "[:alnum:]", @"a-zA-Z0-9" },
        { "[:alpha:]", @"a-zA-Z" },
        { "[:blank:]", @" \t" },
        { "[:cntrl:]", @"\x00-\x1F\x7F" },
        { "[:digit:]", @"0-9" },
        { "[:graph:]", @"\x21-\x7E" },
        { "[:lower:]", @"a-z" },
        { "[:print:]", @"\x20-\x7E" },
        // Note that this has twice the amount of backslashes before the escaped backslash
        // This is because we later replace \\ with \ when building the regex pattern
        { "[:punct:]", @"!""\#$%&'()*+,\-./:;<=>?@\[\\\]^_`{|}~" },
        { "[:space:]", @" \t\r\n\v\f" },
        { "[:upper:]", @"A-Z" },
        { "[:xdigit:]", @"A-Fa-f0-9" }
    };

    var charClasses = charClassSubstitutions.Keys.ToArray();
    
    var patternCharClasses = Regex.Matches(pattern, @"\[\:[a-z]+\:\]").Cast<Match>().Select(m => m.Groups[0].Value);
    
    if (patternCharClasses.Any(pcc => !charClasses.Any(cc => cc == pcc)))
    {
        // Malformed character class
        return false;
    }

    var rx = new StringBuilder(pattern);

    Array.ForEach(literalsToEscapeInRegex, l => rx.Replace(l, @"\" + l));
    Array.ForEach(charClasses, k => rx.Replace(k, charClassSubstitutions[k]));

    rx.Replace("!", "^");

    rx.Insert(0, "^");
    rx.Append("$");

    var rxs = rx.ToString();

    // Remove single backslashes before alphanumeric chars (escaping these should have no effect)
    rxs = Regex.Replace(rxs, @"(?<!\\)\\([a-zA-Z0-9])", "$1");

    // Replace non-escaped star and question mark chars with equivalent regex patterns
    rxs = Regex.Replace(rxs, @"(?<!\\)\*", ".*");
    rxs = Regex.Replace(rxs, @"(?<!\\)\?", ".");

    try
    {
        return !caseSensitive 
            ? Regex.IsMatch(path, rxs, RegexOptions.IgnoreCase)
            : Regex.IsMatch(path, rxs);
    }
    catch
    {
        return false;
    }
}

public class Test
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
    public bool Result { get; set; }
    public bool ResultCI { get; set; }
}