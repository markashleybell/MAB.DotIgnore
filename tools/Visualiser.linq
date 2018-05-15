<Query Kind="Program">
  <Reference Relative="..\MAB.DotIgnore\bin\Debug\netstandard1.3\MAB.DotIgnore.dll">E:\Src\MAB.DotIgnore\MAB.DotIgnore\bin\Debug\netstandard1.3\MAB.DotIgnore.dll</Reference>
  <Namespace>MAB.DotIgnore</Namespace>
  <Namespace>System.Runtime.InteropServices</Namespace>
</Query>

void Main()
{
    var sourceDirectory = @"E:\SOURCE";
    var solutionDirectory = sourceDirectory + @"\TESTSOLUTION";
    var projectDirectory = solutionDirectory + @"\TESTPROJECT";
    var ignoreFile = projectDirectory + @"\.gitignore";
    
    var log = new IgnoreLog();
    
    var ignoreList = new IgnoreList(new string[] { "*.cs" }, MatchFlags.CASEFOLD);
    ignoreList.AddRules(ignoreFile, MatchFlags.CASEFOLD);
    
    var root = InteropDirectoryUtils.ScanDirectory(projectDirectory, projectDirectory);

    var paths = root.Children.DirectoriesFirst()
                             .Expand<PathInfo>(pi => pi.Children.DirectoriesFirst())
                             .ToList();
    
    // paths.Select(pi => pi.Path).Dump();
    paths.ForEach(path => ignoreList.IsIgnored(path.Path, path.IsDirectory, log));
   
    log.ToString().Dump();
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

/// <summary>
/// Abstraction of a filesystem path
/// </summary>
public class PathInfo
{
    public string Path { get; set; }
    public bool IsDirectory { get; set; }

    public List<PathInfo> Children { get; set; }

    public PathInfo()
    {
        Children = new List<PathInfo>();
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct WIN32_FIND_DATAW
{
    public FileAttributes dwFileAttributes;
    internal System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
    internal System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
    internal System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
    public int nFileSizeHigh;
    public int nFileSizeLow;
    public int dwReserved0;
    public int dwReserved1;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public string cFileName;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
    public string cAlternateFileName;
}

public static class InteropDirectoryUtils
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr FindFirstFileW(string lpFileName, out WIN32_FIND_DATAW lpFindFileData);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATAW lpFindFileData);

    [DllImport("kernel32.dll")]
    public static extern bool FindClose(IntPtr hFindFile);
    
    /// <summary>
    /// Recursively scan a directory and build up a lightweight tree structure of child files/directories relative to <paramref name="replacePrefix"/>
    /// </summary>
    /// <remarks>
    /// MUCH Faster file enumeration using Win32 interop calls: modified code from http://stackoverflow.com/a/724184/43140
    /// This method also converts backslashes in paths to forward slashes
    /// </remarks>
    /// <param name="directory">Directory to scan</param>
    /// <param name="replacePrefix">A path prefix which will be removed from the start of all paths added to the tree</param>
    /// <returns></returns>
    public static PathInfo ScanDirectory(string directory, string replacePrefix)
    {
        IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        WIN32_FIND_DATAW findData;
        IntPtr findHandle = INVALID_HANDLE_VALUE;

        var info = new PathInfo {
            Path = NormalisePath(directory.Replace(replacePrefix, "")),
            IsDirectory = true
        };

        try
        {
            findHandle = FindFirstFileW(directory + @"\*", out findData);

            if (findHandle != INVALID_HANDLE_VALUE)
            {
                do
                {
                    if (findData.cFileName == "." || findData.cFileName == "..")
                        continue;

                    string fullpath = directory + (directory.EndsWith("\\") ? "" : "\\") + findData.cFileName;

                    if ((findData.dwFileAttributes & FileAttributes.Directory) != 0)
                    {
                        info.Children.Add(ScanDirectory(fullpath, replacePrefix));
                    }
                    else
                    { 
                        info.Children.Add(new PathInfo {
                            IsDirectory = false,
                            Path = NormalisePath(fullpath.Replace(replacePrefix, ""))
                        });
                    }
                }
                while (FindNextFile(findHandle, out findData));
            }
        }
        finally
        {
            if (findHandle != INVALID_HANDLE_VALUE)
                FindClose(findHandle);
        }

        return info;
    }

    private static string NormalisePath(string path)
    {
        return path.Replace(":", "").Replace(Path.DirectorySeparatorChar.ToString(), "/").Trim().TrimStart('/');
    }
}