/**
 * Created by Jeanma on 14-5-5.
 */
define(["angularAMD","angular-route"],function(angularAMD){
    var app=angular.module("app",["ngRoute"]);

    app.config(["$routeProvider",function($routeProvider){
        $routeProvider.
            when("/home",angularAMD.route({
                templateUrl:"views/home.html",
                controller:"HomeController"
            })).
            otherwise({redirectTo:"/home"});
    }]);
    console.info("define app");
    angularAMD.bootstrap(app);
    return app;
});