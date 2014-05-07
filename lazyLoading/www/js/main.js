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
            'angularAMD': '../../bower_components/angularAMD/angularAMD.min',
            'angular-cookies': '../../bower_components/angular-cookies/angular-cookies.min',
            "N": "../N",
            'jquery': '../../bower_components/jquery/jquery.min',
            'ng-grid': "../../bower_components/ng-grid/ng-grid-2.0.11.min",
            'app':"../app"
        },

        // Add angular modules that does not support AMD out of the box, put it in a shim
        shim: {
            'angular': ["jquery"],
            'angularAMD': ['angular'],
            'angular-route': ['angular'],
            "ng-grid": {
                deps: ["angular"],
                init: function () {
                    loadCss("js/bower_components/ng-grid/ng-grid.min.css");
                }
            },
            "angular-cookies": ["angular"],
            "N": {
                deps:["angular"],
                init:function(){
                    loadCss("js/style/N.css");
                }
            }
        },

        // kick start application
        deps: ['app']
        //set javascript no cache
        , urlArgs: "_=" + getShortDateString()
    });
})();
