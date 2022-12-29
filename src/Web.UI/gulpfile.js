/// <binding ProjectOpened='debug' />
/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require('gulp'),
    bower = require('gulp-bower'),
    less = require('gulp-less'),
    watch = require('gulp-watch'),
    uglify = require('gulp-uglify'),
    rename = require('gulp-rename'),
    concat = require('gulp-concat'),
    bowerDir = './bower_components/',
    outputDir = './app/',
    nuGetDir = './Scripts/',
    scripts = [],
    css = [];

scripts = [
    bowerDir + "jquery/dist/jquery.js",
    bowerDir + "jquery-validation/dist/*.js",
    bowerDir + "jquery-ui/*.js",
    bowerDir + "jquery-form/jquery.form.js",
    bowerDir + "jQuery-File-Upload/jquery.fileupload.js",
    bowerDir + "Microsoft.jQuery.Unobtrusive.Validation/jquery.validate.unobtrusive.js",
    bowerDir + "moment/*.js",
    bowerDir + 'bootstrap/dist/js/bootstrap.js',
    bowerDir + "bootstrap-less/js/*.js",
    bowerDir + "bootstrap-select/dist/js/bootstrap-select.min.js",
    bowerDir + "eonasdan-bootstrap-datetimepicker/src/js/bootstrap-datetimepicker.js",
    bowerDir + "fancyBox/source/helpers/*.js",
    bowerDir + "fancyBox/source/*.js",
    bowerDir + "blueimp-gallery/js/*.js",
    bowerDir + "jstree/dist/*.js",
    bowerDir + "knockout/dist/knockout.js",
    bowerDir + "knockout-validation/dist/knockout.validation.js",
    bowerDir + "knockout-mapping/knockout.mapping.js",
    bowerDir + "alertify/alertify.js",
    bowerDir + "hopscotch/dist/js/hopscotch.min.js",
    bowerDir + "bootstrap-toggle/js/bootstrap-toggle.js",
    bowerDir + 'bootstrap-hover-dropdown/bootstrap-hover-dropdown.js',
    bowerDir + "datatables.net/js/jquery.dataTables.min.js",
    bowerDir + "raphael/raphael.min.js",
    bowerDir + "morris.js/morris.min.js",
    bowerDir + "notifit/notifIt/js/notifIt.min.js",
    bowerDir + 'ContentTools/build/*.js',
    bowerDir + 'typeahead.js/dist/typeahead.bundle.js',
    bowerDir + 'bootstrap-tagsinput/dist/bootstrap-tagsinput.js',
    nuGetDir + '*.js'
];
css = [
    bowerDir + '**/*.css',
    bowerDir + '**/*.less',
    bowerDir + '**/*.png',
    bowerDir + '**/*.svg',
    bowerDir + '**/*.woff',
    bowerDir + '**/*.gif',
    './Content/' + 'jquery.dataTables.css'
];

gulp.task('bower-install', function () {
    return bower();
});
gulp.task('copyScriptsFromBower', ['bower-install'], function () {
    return gulp.src(scripts)
           .pipe(gulp.dest('./Scripts'));
});
gulp.task('copyCssToOutputDir', ['bower-install'], function () {
    return gulp.src(css)
           .pipe(gulp.dest('./Content'));
});
gulp.task('less', ['copyCssToOutputDir'], function () {
    return gulp.src(['./Content/admin.less'])
            .pipe(less())
            .pipe(gulp.dest('./Content/'));
});
gulp.task('less-debug', function () {
    return gulp.src(['./Content/admin.less'])
            .pipe(less())
            .pipe(gulp.dest('./Content/'));
});
gulp.task('fonts', ['bower-install'], function () {
    return gulp.src([bowerDir + 'fontawesome/fonts/*', bowerDir + 'bootstrap/fonts/*', bowerDir + 'ContentTools/build/images/*'])
           .pipe(gulp.dest('./fonts'));
});
gulp.task('watch', function () {
    return gulp.watch('./Content/*.less', ['less-debug']);
});
gulp.task('release', ['bower-install', 'copyScriptsFromBower', 'copyCssToOutputDir', 'fonts', 'less']);
gulp.task('debug', ['bower-install', 'copyScriptsFromBower', 'copyCssToOutputDir', 'fonts', 'less', 'watch']);

