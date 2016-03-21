<Query Kind="Program">
  <Reference Relative="IgnoreSharp\bin\Debug\IgnoreSharp.dll">E:\Inetpub\myapps\IgnoreSharp\IgnoreSharp\bin\Debug\IgnoreSharp.dll</Reference>
  <Namespace>IgnoreSharp</Namespace>
</Query>

void Main()
{

    var ignoreList = new IgnoreList(new List<string> { "*.txt", "!**/test2/*.txt" });

    ignoreList.Rules.Dump();

    var paths = new List<string> { 
        "README.txt",
        "test/README.txt",
        "test/test2/README.txt",
        "test1/test3/test2/README.txt",
        "test1/test3/test2/README",
        "test2/README/readme.txt",
        "test3/test4/README/readme.txt",
        "README.TXT"
    };

    paths.ForEach(path => ignoreList.IsMatch(path).Dump("Match '" + path + "'"));
    
}

