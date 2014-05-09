/**
 * Created by jm96 on 14-5-7.
 */
define(["app"],function(app){
    app.register.controller("dataGrid1Controller",function($scope){
        $scope.data=[{
            id:1,
            name:"jean",
            age:1
        },{
            id:2,
            name:"eva",
            age:2
        }];
    });
});