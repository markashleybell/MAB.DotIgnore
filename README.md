# IgnoreSharp

IgnoreSharp allows you to load a `.gitignore` file (or any text file using the same syntax), then use its rules to determine whether a particular file path should be ignored.

## Basic usage: 

#### `.gitignore` contents

    *.txt

#### C# code

    var ignores = new IgnoreList(@"path\to\my\.gitignore");
    
    ignores.IsMatch(@"path\to\ignore.txt"); // Returns true
    ignores.IsMatch(@"path\to\include.cs"); // Returns false

