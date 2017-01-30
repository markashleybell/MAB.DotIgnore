<Query Kind="Program">
  <NuGetReference>MAB.DotIgnore</NuGetReference>
  <NuGetReference>Webselect.Lib.IO</NuGetReference>
  <Namespace>MAB.DotIgnore</Namespace>
  <Namespace>Webselect.Lib.IO</Namespace>
</Query>

void Main()
{
    var sourceDirectory = @"E:\Inetpub\wwwroot\specialtyfasteners.co.uk\specialtyfasteners.co.uk.CMS";
    var ignoreFile = @"E:\Inetpub\wwwroot\specialtyfasteners.co.uk\.wsdignore";
    
    var log = new IgnoreLog();
    
    var ignoreList = new IgnoreList(new string[] { "*.cs" }, MatchFlags.CASEFOLD);
    ignoreList.AddRules(ignoreFile, MatchFlags.CASEFOLD);
    
    var root = InteropDirectoryUtils.ScanDirectory(sourceDirectory, sourceDirectory);

    var paths = root.Children.DirectoriesFirst()
                             .Expand<PathInfo>(pi => pi.Children.DirectoriesFirst())
                             .ToList();
    
    // paths.Select(pi => pi.Path).Dump();
    
    paths.ForEach(path => ignoreList.IsIgnored(path.Path, path.IsDirectory, log));
   
    log.ToString().Dump();
}

public class TestPath
{
    public string Path { get; set; }
    public bool IsDirectory { get; set; }
}

public static class Extensions 
{
    public static IEnumerable<PathInfo> DirectoriesFirst(this IEnumerable<PathInfo> source)
    {
        return source.OrderByDescending(i => i.IsDirectory);
    }
    
    // See http://stackoverflow.com/a/31881243/43140
    public static IEnumerable<T> Expand<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> elementSelector)
    {
        var stack = new Stack<IEnumerator<T>>();
        var e = source.GetEnumerator();
        try
        {
            while (true)
            {
                while (e.MoveNext())
                {
                    var item = e.Current;
                    yield return item;
                    var elements = elementSelector(item);
                    if (elements == null) continue;
                    stack.Push(e);
                    e = elements.GetEnumerator();
                }
                if (stack.Count == 0) break;
                e.Dispose();
                e = stack.Pop();
            }
        }
        finally
        {
            e.Dispose();
            while (stack.Count != 0) stack.Pop().Dispose();
        }
    }
}