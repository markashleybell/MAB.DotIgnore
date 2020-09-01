
$NuGetPackages = "${env:USERPROFILE}\.nuget\packages"
$TestProject = "MAB.DotIgnore.Test.csproj"
$ReportFolder = "coverage"

New-Item -Path . -Type Directory -Name $ReportFolderName -Force | Out-Null

&"$NuGetPackages\opencover\4.7.922\tools\OpenCover.Console.exe" `
    -target:"C:\Program Files\dotnet\dotnet.exe" `
    -targetargs:"test -f netcoreapp3.1 -c Debug $TestProject" `
    -excludebyattribute:*.excludefromcodecoverage* `
    -filter:"+[MAB.DotIgnore*]* -[MAB.DotIgnore.Test*]* -[MAB.DotIgnore]MAB.DotIgnore.ExcludeFromTestCoverageAttribute" `
    -register:user `
    -oldStyle `
    -output:"$ReportFolder\results.xml" `
    -searchdirs:"bin\debug\netcoreapp3.1"

&"$NuGetPackages\reportgenerator\4.6.5\tools\net47\ReportGenerator.exe" `
    -reports:"$ReportFolder\results.xml" `
    -targetdir:"$ReportFolder"
