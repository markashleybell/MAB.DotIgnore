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
        .Select(l => new Test { 
            Pattern = l[6].Trim('\''),
            Path = l[5].Trim('\''),
            ExpectGlobMatch = l[1] == "1",
            ExpectGlobMatchCI = l[2] == "1",
            ExpectPathMatch = l[3] == "1",
            ExpectPathMatchCI = l[4] == "1"
        });
        
    // tests.Dump();
    
    var expected = tests.Select((t, i) => new Check {
        ID = i,
        Pattern = t.Pattern,
        Path = t.Path,
        Result = t.ExpectPathMatchCI
    });
    
    var actual = tests.Select((t, i) => new Check {
        ID = i,
        Pattern = t.Pattern,
        Path = t.Path,
        Result = Match(t.Pattern, t.Path)
    });
    
    // Util.Dif(expected, actual).Dump();
    
    var failed = actual.Where(a => a.Result != expected.Single(e => e.ID == a.ID).Result)
        .Select(a => new { a.Pattern, a.Path, Expected = !a.Result, Actual = a.Result });
    
    failed.Dump();
    
    Match(pattern: @"foo\*", path: "foo*", dumpRegex: true).Dump();
}

public bool Match(string pattern, string path, bool caseSensitive = false, bool dumpRegex = false)
{
    var literals = new[] { @"\", ".", "$", "{", "}", "(", "|", ")", "+" };
    
    var charClasses = new Dictionary<string, string> {
        { "[:alnum:]", "a-z0-9" },
        { "[:alpha:]", "a-z" },
        { "[:blank:]", "" },
        { "[:cntrl:]", "" },
        { "[:digit:]", @"0-9" },
        { "[:graph:]", "" },
        { "[:lower:]", "a-z" },
        { "[:print:]", "" },
        { "[:punct:]", "" },
        { "[:space:]", @"\s" },
        { "[:upper:]", "A-Z" },
        { "[:xdigit:]", "" }
    };
    
    var rx = new StringBuilder(pattern);

    Array.ForEach(literals, l => rx.Replace(l, @"\" + l));
    Array.ForEach(charClasses.Keys.ToArray(), k => rx.Replace(k, charClasses[k]));

    rx.Replace("*", ".*");
    rx.Replace(@"\\.*", @"\*");
    rx.Replace("?", ".");
    rx.Replace("!", "^");

    rx.Insert(0, "^");
    rx.Append("$");

    var rxs = rx.ToString();

    // rxs = Regex.Replace(rxs, @"[^\\]\*", ".*");
    
    if (dumpRegex)
        rxs.Dump();

    try
    {

        return Regex.IsMatch(path, rxs, RegexOptions.IgnoreCase);
    }
    catch
    {
        return false;
    }
}

public class Test
{
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
}