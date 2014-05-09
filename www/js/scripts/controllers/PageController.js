/**
 * Created by jm96 on 14-5-7.
 */
define(["app"],function(app){
    debugger
    var PagesController= app.register.controller("PageController",
        function($scope,$http,$route,$routeParams,$compile){
        $route.current.templateUrl = 'views/' + $routeParams.name + ".html";

        $http.get($route.current.templateUrl).then(function (msg) {
            $('#views').html($compile(msg.data)($scope));
        });
    });

    PagesController.$inject = ['$scope', '$http', '$route', '$routeParams', '$compile'];
});