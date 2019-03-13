[CmdletBinding()]

param(
    [Parameter()] 
    $Mode = 's',
    [switch]
    $RunBaseline
)

$ArtifactsPath = 'BenchmarkDotNet.Artifacts'

Remove-Item -Path $ArtifactsPath -Recurse -Force -ErrorAction SilentlyContinue

if ($RunBaseline) {
    &"..\baseline\bin\Release\net472\baseline.exe" $Mode
}

&"..\benchmark\bin\Release\net472\benchmark.exe" $Mode

$Template = Get-Content 'benchmark-report-template.html'

$ResultsPath = "$ArtifactsPath\results"

$Results = 
    Get-ChildItem $ResultsPath -File -Filter "*report.html" | 
    Select-Object -ExpandProperty FullName |
    ForEach-Object { Get-Content $_ -Raw } |
    ForEach-Object { $_ -match '(?s)<body>(.*)</body>' } |
    ForEach-Object { $Matches[1] }

$Now = Get-Date

$AllResults = $Results -join ""

$Html = "Generated: $Now<br>$AllResults"

$Template -replace "\{REPORT\}", $Html | Set-Content -Path 'benchmark-results.html'
