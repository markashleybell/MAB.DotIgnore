@ECHO OFF

ECHO Report generation takes approximately 3 minutes to run...

"%UserProfile%\.nuget\packages\opencover\4.6.519\tools\OpenCover.Console.exe" ^
-target:"C:\Program Files\dotnet\dotnet.exe" ^
-targetargs:"test -f netcoreapp2.0 -c Debug ../MAB.DotIgnore.Test/MAB.DotIgnore.Test.csproj" ^
-excludebyattribute:*.ExcludeFromTestCoverageAttribute ^
-filter:"+[MAB.DotIgnore*]* -[MAB.DotIgnore.Test*]* -[MAB.DotIgnore]MAB.DotIgnore.ExcludeFromTestCoverageAttribute" ^
-register:user ^
-oldStyle ^
-output:"..\reports\results.xml" ^
-searchdirs:"..\MAB.DotIgnore.Test\bin\Debug\netcoreapp2.0"

"%UserProfile%\.nuget\packages\reportgenerator\4.0.2\tools\net47\ReportGenerator.exe" ^
-reports:"..\reports\results.xml" ^
-targetdir:"..\reports"
