# MAB.DotIgnore

Load and parse `.gitignore` files (or any text files using the same syntax) to produce an `IgnoreList` which can be used to ignore specific files and folders during (for example) a recursve file copy operation.

The goal of this library is to implement the same file matching behaviour which [Git](https://github.com/git/git) uses (to determine which files to ignore when adding to a commit). Please feel free to submit [issues](https://github.com/markashleybell/MAB.DotIgnore/issues) with examples—or pull requests—if you find instances where this isn't the case!

## Basic usage: 

#### `.gitignore` file contents

    *.txt

#### C# code

```cs
var ignores = new IgnoreList(@"path\to\my\.gitignore");

ignores.IsIgnored(@"path\to\ignore.txt"); // Returns true
ignores.IsIgnored(@"path\to\include.cs"); // Returns false
```

#### Example usage

A quick example illustrating how you might integrate an `IgnoreList` into a copy routine:

```cs
public static void CopyWithIgnores(DirectoryInfo source, DirectoryInfo target, IgnoreList ignores)
{
    foreach (DirectoryInfo dir in source.GetDirectories().Where(d => !ignores.IsIgnored(d)))
        CopyWithIgnores(dir, target.CreateSubdirectory(dir.Name), ignores);

    foreach (FileInfo file in source.GetFiles().Where(f => !ignores.IsIgnored(f)))
        file.CopyTo(Path.Combine(target.FullName, file.Name));
}

var source = new DirectoryInfo(@"c:\source");
var destination = new DirectoryInfo(@"c:\destination");

var ignores = new IgnoreList(@"c:\source\.gitignore");

CopyWithIgnores(source, destination, ignores);
```
