/**
 * Created by Jeanma on 14-5-5.
 */
define(["app"],function(app){
    app.register.controller("HomeController",["$scope",function($scope){
        $scope.random=Math.random();
    }]);
});