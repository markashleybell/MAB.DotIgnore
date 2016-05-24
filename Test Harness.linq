<Query Kind="Program">
  <Reference Relative="IgnoreSharp\bin\Debug\IgnoreSharp.dll">E:\Inetpub\myapps\IgnoreSharp\IgnoreSharp\bin\Debug\IgnoreSharp.dll</Reference>
  <Namespace>IgnoreSharp</Namespace>
</Query>

void Main()
{
    var paths = new List<string> {
        "test1.jpg",
        "test/test1.jpg",
        "test/test2.jpg",
        "test/sub/test3.jpg"
    };

    var ignorePatterns = new string[] {
        "*.jpg",
        "!sub/*.jpg"
    };
    
    var ignoreList = new IgnoreList(ignorePatterns);
    paths.ForEach(path => ignoreList.IsMatch(path, false).Dump("Match '" + path + "'"));
    
	var ignoreRule = new IgnoreRule("test/*.jpg");
    paths.ForEach(path => ignoreRule.IsMatch(path, false).Dump("Match '" + path + "'"));
}