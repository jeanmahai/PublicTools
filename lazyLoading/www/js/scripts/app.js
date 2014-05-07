/**
 * Created by Jeanma on 14-5-5.
 */
define(["angularAMD"
        , "angular-route"
        , "ng-grid"
        , "angular-cookies"
        , "N"], function (angularAMD) {

    var app = angular.module("app", ["ngRoute", "ngGrid", "ngCookies", "NProvider"]);

    //config $N
    app.run(function ($N) {
        $N.showLoading = true;
    });

    //config url route
    app.config(["$routeProvider", function ($routeProvider) {
        $routeProvider.
            when("/home", angularAMD.route({
                templateUrl: "views/home.html",
                controller: "HomeController"
            })).
            when("/demo", angularAMD.route({
                templateUrl: "views/dynamicLoadControllerAndView.html", controller: "DynamicController"
            })).
            otherwise({redirectTo: "/home"});
    }]);

    //interceptor http
    app.factory("httpInterceptor", ["$N", function ($N) {
        return{
            'request': function (config) {
                // do something on success
                //处理自定义headers
                config.headers = {
                    "x-newegg-mobile-cookie": window.localStorage.getItem("x-newegg-mobile-cookie")
                };
                //处理loading
                $N.loading(config);
                return config || $q.when(config);
            },
            'response': function (response) {
                // do something on success
                //处理自定义的headers
                var mobileCookie = response.headers("x-newegg-mobile-cookie");
                if (mobileCookie && mobileCookie != "") {

                }
                window.localStorage.setItem("x-newegg-mobile-cookie", mobileCookie);
                //处理loaded
                $N.loaded(response);

                return response || $q.when(response);
            }
        };
    }]);
    app.config(function ($httpProvider) {
        $httpProvider.interceptors.push("httpInterceptor");
    });


    //start
    angularAMD.bootstrap(app);
    return app;
});