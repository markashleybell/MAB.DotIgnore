/// <binding AfterBuild='nuget-pack' />
"use strict";

var gulp = require('gulp'),
    path = require('path'),
    exec = require('child_process').exec,
    fs = require('fs');

var solutionFolder = path.resolve(__dirname, '..');
var distFolder = path.join(solutionFolder, 'dist');

if (!fs.existsSync(distFolder)) {
    fs.mkdirSync(distFolder);
}

gulp.task('nuget-pack', function (callback) {
    exec("nuget pack MAB.DotIgnore.nuspec -OutputDirectory " + distFolder, { cwd: solutionFolder }, function (err, stdout, stderr) {
        console.log(stdout);
        console.log(stderr);
        callback(err);
    });
});
