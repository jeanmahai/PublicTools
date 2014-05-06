/**
 * Created by Jeanma on 14-5-5.
 */
(function () {
    function getShortDateString(){
        var date=new Date();
        //cache 1 hour
        //return date.getFullYear()+date.getMonth()+date.getDate()+date.getHours();

        //no cache
        return Math.random();
    }
    require.config({
        baseUrl: "js/scripts",
        paths: {
            'angular': '../bower_components/angular/angular.min',
            'angular-route': '../bower_components/angular-route/angular-route.min',
            'angularAMD': '../bower_components/angularAMD/angularAMD.min',
            'HomeController': 'controllers/HomeController',
            'jquery': '../bower_components/jquery/jquery.min', 'DynamicController': "controllers/DynamicController", 'ng-grid': "../bower_components/ng-grid/ng-grid-2.0.11.min"
        },

        // Add angular modules that does not support AMD out of the box, put it in a shim
        shim: {
            'angular': ["jquery"],
            'angularAMD': ['angular'],
            'angular-route': ['angular'], "ng-grid": ["angular"]
        },

        // kick start application
        deps: ['app']
        //set javascript no cache
        , urlArgs: "_=" + getShortDateString()
    });
})();
