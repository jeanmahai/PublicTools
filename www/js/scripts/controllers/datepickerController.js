/**
 * Created by jm96 on 14-5-9.
 */
define(["app"],function(app){
    app.register.controller("datepickerController",function($scope){
        $scope.dateValue=new Date();
    });
});