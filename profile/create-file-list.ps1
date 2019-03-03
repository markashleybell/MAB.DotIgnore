
param (
    [Parameter(Mandatory=$true)]
    [string]
    $SolutionPath
)

Get-ChildItem -Recurse $SolutionPath -File | 
ForEach-Object { $_.FullName } | 
Out-File .\data\filelist.txt -Encoding utf8 -Force
