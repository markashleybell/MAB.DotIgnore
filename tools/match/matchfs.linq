<Query Kind="FSharpProgram">
  <NuGetReference>FParsec</NuGetReference>
  <Namespace>FParsec</Namespace>
  <Namespace>System.Runtime.InteropServices</Namespace>
</Query>

// https://github.com/git/git/blob/master/t/t3070-wildmatch.sh

type PatternTest = {
    Path: string
    Pattern: string
    ExpectGlobMatch: bool
    ExpectGlobMatchCI: bool
    ExpectPathMatch: bool
    ExpectPathMatchCI: bool
}

let workingDirectory = Path.GetDirectoryName Util.CurrentQueryPath

let isCommentLine (l: string) = l.StartsWith("#")
let isEmptyLine l = String.IsNullOrWhiteSpace(l)
let isNotEmptyOrComment l = not (l |> isEmptyLine || l |> isCommentLine)
let splitLineAtSpaces (l: string) = l.Split([|' '|], StringSplitOptions.RemoveEmptyEntries)

let asPatternTest (l: string[]) = 
    { 
        Path = l.[5].Trim('\'')
        Pattern = l.[6].Trim('\'')
        ExpectGlobMatch = l.[1] = "1"
        ExpectGlobMatchCI = l.[2] = "1"
        ExpectPathMatch = l.[3] = "1"
        ExpectPathMatchCI = l.[4] = "1"
    }

let tests = 
    File.ReadAllLines(workingDirectory + @"\tests.txt")
    |> Array.filter isNotEmptyOrComment
    |> Array.map splitLineAtSpaces
    |> Array.map asPatternTest
    
tests |> Dump |> ignore
