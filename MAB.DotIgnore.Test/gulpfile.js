"use strict";

const gulp = require('gulp');
const path = require('path');
const glob = require("glob");
const exec = require('child_process').exec;
const fs = require('fs');

const  solutionFolder = path.resolve(__dirname, '..');
const  nugetPackageFolder = path.join(solutionFolder, 'packages');
const  reportsFolder = '../reports';

if (!fs.existsSync(reportsFolder)) {
    fs.mkdirSync(reportsFolder);
}

const globOptions = {
    cwd: nugetPackageFolder,
    nocase: true,
    nodir: true,
    realpath: true
};

const nunitExecutable = glob.sync("NUnit.ConsoleRunner*/tools/nunit3-console.exe", globOptions)[0];
const openCoverExecutable = glob.sync("OpenCover*/tools/OpenCover.Console.exe", globOptions)[0];
const reportGeneratorExecutable = glob.sync("ReportGenerator*/tools/ReportGenerator.exe", globOptions)[0];

const openCoverArgs = [
    '-target:"' + nunitExecutable + '"',
    '-targetargs:"/out:\\"' + path.join('reports/testresults.xml') + '\\" ' + path.join('MAB.DotIgnore.Test/bin/Debug/MAB.DotIgnore.Test.dll') + '"',
    '-excludebyattribute:*.ExcludeFromTestCoverageAttribute',
    '-filter:"+[*]* -[MAB.DotIgnore.Test*]* -[MAB.DotIgnore]MAB.DotIgnore.ExcludeFromTestCoverageAttribute"',
    '-register:user',
    '-output:"' + path.join('reports/results.xml') + '"'
];

const openCoverCommand = openCoverExecutable + ' ' + openCoverArgs.join(' ');

const reportGeneratorArgs = [
    '-reports:"' + path.join('reports/results.xml') + '"',
    '-targetdir:"reports"'
];

const reportGeneratorCommand = reportGeneratorExecutable + ' ' + reportGeneratorArgs.join(' ');

gulp.task('opencover-reports', callback => {
    exec(openCoverCommand, { cwd: solutionFolder }, (ocerr, ocstdout, ocstderr) => {
        console.log(ocstdout);
        console.log(ocstderr);
        // If nothing went wrong with the OpenCover run, generate the report files
        if(!ocerr) {
            exec(reportGeneratorCommand, { cwd: solutionFolder }, (rgerr, rgstdout, rgstderr) => {
                console.log(rgstdout);
                console.log(rgstderr);
                callback(rgerr);
            });
        } else {
            callback(ocerr);
        }
    });
});
