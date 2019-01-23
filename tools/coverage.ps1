
$NuGetPackages = "${env:USERPROFILE}\.nuget\packages"
$TestProjectFolderPath = "..\MAB.DotIgnore.Test"
$TestProject = "$TestProjectFolderPath\MAB.DotIgnore.Test.csproj"
$ReportFolderName = "coverage"
$ReportFolderPath = "..\$ReportFolderName"

New-Item -Path ..\ -Type Directory -Name $ReportFolderName -Force | Out-Null

&"$NuGetPackages\opencover\4.6.519\tools\OpenCover.Console.exe" `
    -target:"C:\Program Files\dotnet\dotnet.exe" `
    -targetargs:"test -f netcoreapp2.0 -c Debug $TestProject" `
    -excludebyattribute:*.excludefromcodecoverage* `
    -filter:"+[MAB.DotIgnore*]* -[MAB.DotIgnore.Test*]* -[MAB.DotIgnore]MAB.DotIgnore.ExcludeFromTestCoverageAttribute" `
    -register:user `
    -oldStyle `
    -output:"$ReportFolderPath\results.xml" `
    -searchdirs:"$TestProjectFolderPath\bin\debug\netcoreapp2.0"

&"$NuGetPackages\reportgenerator\4.0.8\tools\net47\ReportGenerator.exe" `
    -reports:"$ReportFolderPath\results.xml" `
    -targetdir:"$ReportFolderPath"