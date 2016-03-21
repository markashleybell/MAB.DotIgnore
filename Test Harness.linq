<Query Kind="Program">
  <Reference Relative="IgnoreSharp\bin\Debug\IgnoreSharp.dll">E:\Inetpub\myapps\IgnoreSharp\IgnoreSharp\bin\Debug\IgnoreSharp.dll</Reference>
  <Namespace>IgnoreSharp</Namespace>
</Query>

void Main()
{

    var ignoreList = new IgnoreList(new List<string> { "folder/" });

    ignoreList.Rules.Dump();

    var paths = new List<string> { 
        "folder",
        "folders",
        "sub1/folder",
        "sub1/folders",
        "sub1/sub2/folder",
        "sub1/sub2/folders",
        "sub1/folder/sub2",
        "sub1/folders/sub2"
    };

    paths.ForEach(path => ignoreList.IsMatch(path).Dump("Match '" + path + "'"));
    
}

