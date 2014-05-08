/**
 * Created by Jeanma on 14-5-5.
 */
define(["angularAMD"
    , "angular-route"
    //, "ng-grid"
    , "angular-cookies"
    , "angular-date"
    , "N"], function (angularAMD) {

    var app = angular.module("app", ["ngRoute"
        //, "ngGrid"
        , "ngCookies"
        , "NProvider"
        , "ui.date"]);

    //config $N
    app.run(function ($N) {
        $N.showLoading = true;
    });

    //config url route
    //================================
    //动态routing,目前已经禁用了,由于功能还不成熟,暂停使用
    //需要做的东西::controller/:view/:params
    //1.动态去加载对应的view
    //2.动态去加载对应的controller
    //3.处理参数
    //modify angularAMD line angularAMD.prototype.route, line 108~111
    //================================
    app.config(["$routeProvider", function ($routeProvider) {
        $routeProvider.
            when("/home",angularAMD.route({
                templateUrl:"views/home.html",
                controller:"homeController"
            })).
            when("/datagrid1",angularAMD.route({
                templateUrl:"views/dataGrid1.html",
                controller:"dataGrid1Controller"
            })).
            otherwise({redirectTo:"/home"});
//            when("/:name", angularAMD.route({
//                templateUrl: function($routeParams){
//                    var tempUrl='views/' + $routeParams.name + ".html";
//                    return tempUrl;
//                },
//                controller: function(){
//                    console.info(window.location.href);
//                    var index=window.location.href.lastIndexOf("/");
//                    var name=window.location.href.substring(index+1);
//                    return name+"Controller";
//                }
//            }));
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