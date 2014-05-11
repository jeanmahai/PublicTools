/**
 * Created by Jeanma on 14-5-5.
 */
(function () {
    function getShortDateString() {
        var date = new Date();
        //cache 1 hour
        //return date.getFullYear()+date.getMonth()+date.getDate()+date.getHours();

        //no cache
        return Math.random();
    }

    function loadCss(url) {
        var link = document.createElement("link");
        link.type = "text/css";
        link.rel = "stylesheet";
        link.href = url;
        document.getElementsByTagName("head")[0].appendChild(link);
    }

    require.config({
        baseUrl: "js/scripts/controllers",
        paths: {
            'angular': '../../bower_components/angular/angular.min',
            'angular-route': '../../bower_components/angular-route/angular-route.min',
            'angularAMD': '../../bower_components/angularAMD/angularAMD',
            'angular-cookies': '../../bower_components/angular-cookies/angular-cookies.min',
            "N": "../N",
            'jquery': '../../bower_components/jquery/jquery.min',
            'ng-grid': "../../bower_components/ng-grid/ng-grid-2.0.11.min",
            'app': "../app",
            'config': "../config",
            'jquery-ui': "../../bower_components/jquery-ui/ui/jquery-ui",
            'jquery-ui-core': "../../bower_components/jquery-ui/ui/minified/jquery.ui.core.min",
            'jquery-ui-datepicker': "../../bower_components/jquery-ui/ui/minified/jquery.ui.datepicker.min",
            "angular-date": "../../bower_components/angular-ui-date/src/date",
            "jquery-ui-datepicker-zh-cn": "../../bower_components/jquery-ui/ui/i18n/jquery.ui.datepicker-zh-CN",
            "pageLoaded": "../pageLoaded"
        },

        // Add angular modules that does not support AMD out of the box, put it in a shim
        shim: {
            'angular': {
                deps: ["pageLoaded","jquery"],
                init: function () {
                    //loadCss("js/bower_components/pure/pure-min.css");
                }
            },
            "pageLoaded":["jquery"],
            'angularAMD': ['angular'],
            'angular-route': ['angular'],
            "ng-grid": {
                deps: ["angular", "jquery"],
                init: function () {
                    loadCss("js/bower_components/ng-grid/ng-grid.min.css");
                }
            },
            "angular-cookies": ["angular"],
            "N": {
                deps: ["angular"],
                init: function () {
                    loadCss("css/N.css");
                }
            },
            "jquery-ui-core": ["jquery"],
            "jquery-ui-datepicker-zh-cn": ["jquery-ui-core", "jquery-ui-datepicker"],
            "jquery-ui-datepicker": ["jquery-ui-core"],
            "jquery-ui": ["jquery"],
            "angular-date": {
                deps: ["angular", "jquery-ui-core", "jquery-ui-datepicker"],
                init: function (a, b, c) {
                    loadCss("js/bower_components/jquery-ui/themes/ui-darkness/jquery-ui.min.css");
                }
            },
            'app': ["config"]

        },

        // kick start application
        deps: ['app']
        //set javascript no cache
        , urlArgs: "_=" + getShortDateString()
    });
})();
