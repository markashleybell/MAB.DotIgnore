<Query Kind="Program">
  <Reference Relative="MAB.DotIgnore\bin\Debug\MAB.DotIgnore.dll">E:\Inetpub\myapps\IgnoreSharp\MAB.DotIgnore\bin\Debug\MAB.DotIgnore.dll</Reference>
  <Namespace>MAB.DotIgnore</Namespace>
</Query>

void Main()
{
    var paths = new List<string> {
        "test1.jpg",
        "test/test1.jpg",
        "test/test2.jpg",
        "test/sub/test3.jpg",
        "test/sub/test3.jpg",
        "test/sub/f123.jpg"
    };

    var ignorePatterns = new string[] {
        "sub/",
        "*.jpg",
        "!sub/*.jpg",
        "sub/f*.jpg"
    };
    
    var ignoreList = new IgnoreList(ignorePatterns);
    paths.ForEach(path => ignoreList.IsMatch(path, false).Dump("Ignore '" + path + "'"));
    
	var ignoreRule = new IgnoreRule("test/*.jpg");
    paths.ForEach(path => ignoreRule.IsMatch(path, false).Dump("Ignore '" + path + "'"));
}