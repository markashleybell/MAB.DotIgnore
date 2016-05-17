var gulp = require('gulp'),
    exec = require('child_process').exec;

gulp.task('task', function (callback) {
    // TODO: Run OpenCover and ReportGenerator here
    exec('ping localhost', function (err, stdout, stderr) {
        //console.log(stdout);
        //console.log(stderr);
        callback(err);
    });
});