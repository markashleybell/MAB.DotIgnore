<Query Kind="Program">
  <Reference Relative="IgnoreSharp\bin\Debug\IgnoreSharp.dll">C:\Src\IgnoreSharp\IgnoreSharp\bin\Debug\IgnoreSharp.dll</Reference>
  <Namespace>IgnoreSharp</Namespace>
</Query>

void Main()
{
	var ignoreRule = new IgnoreRule("test/*.jpg");

    var paths = new List<string> { 
        "test1.jpg",
		"test/test1.jpg",
		"test/test2.jpg",
		"test/sub/test3.jpg"
    };

    paths.ForEach(path => ignoreRule.IsMatch(path, false).Dump("Match '" + path + "'"));
}