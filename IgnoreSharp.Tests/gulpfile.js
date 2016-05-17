"use strict";

var gulp = require('gulp'),
    path = require('path'),
    glob = require("glob"),
    exec = require('child_process').exec,
    fs = require('fs');

var solutionFolder = path.resolve(__dirname, '..');
var nugetPackageFolder = path.join(solutionFolder, 'packages');
var reportsFolder = '../reports';

if (!fs.existsSync(reportsFolder)) {
    fs.mkdirSync(reportsFolder);
}

var globOptions = {
    cwd: nugetPackageFolder,
    nocase: true,
    nodir: true,
    realpath: true
};

var nunitExecutable = glob.sync("NUnit.ConsoleRunner*/tools/nunit3-console.exe", globOptions)[0];
var openCoverExecutable = glob.sync("OpenCover*/tools/OpenCover.Console.exe", globOptions)[0];
var reportGeneratorExecutable = glob.sync("ReportGenerator*/tools/ReportGenerator.exe", globOptions)[0];

var openCoverArgs = [
    '-target:"' + nunitExecutable + '"',
    '-targetargs:"/out:\\"' + path.join('reports/testresults.xml') + '\\" ' + path.join('IgnoreSharp.Tests/bin/Debug/IgnoreSharp.Tests.dll') + '"',
    '-excludebyattribute:*.ExcludeFromTestCoverageAttribute',
    '-filter:"+[*]* -[IgnoreSharp.Tests*]* -[IgnoreSharp]IgnoreSharp.ExcludeFromTestCoverageAttribute"',
    '-register:user',
    '-output:"' + path.join('reports/results.xml') + '"'
];

var openCoverCommand = openCoverExecutable + ' ' + openCoverArgs.join(' ');

var reportGeneratorArgs = [
    '-reports:"' + path.join('reports/results.xml') + '"',
    '-targetdir:"reports"'
];

var reportGeneratorCommand = reportGeneratorExecutable + ' ' + reportGeneratorArgs.join(' ');

gulp.task('task', function (callback) {
    exec(openCoverCommand, { cwd: solutionFolder }, function (ocerr, ocstdout, ocstderr) {
        console.log(ocstdout);
        console.log(ocstderr);
        // If nothing went wrong, generate the report files
        if(!ocerr) {
            exec(reportGeneratorCommand, { cwd: solutionFolder }, function (rgerr, rgstdout, rgstderr) {
                console.log(rgstdout);
                console.log(rgstderr);
                callback(rgerr);
            });
        } else {
            callback(ocerr);
        }
    });
});