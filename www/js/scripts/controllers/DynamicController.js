/**
 * Created by jm96 on 14-5-6.
 */
define(["app"], function (app) {
    app.register.controller("DynamicController",
        ["$scope", "$cookieStore", "$http", "$cookies",
            function ($scope, $cookieStore, $http, $cookies) {
                $scope.data = [
                    {name: "name1", age: 1, id: 1},
                    {name: "name2", age: 2, id: 2}
                ];
                $scope.gridOptions = {
                    data: 'data', columnDefs: [
                        {
                            displayName: "姓名", field: "name"
                        },
                        {
                            displayName: "年龄", field: "age"
                        }
                    ]
                };

                $scope.keyval = {
                    key: "",
                    val: ""
                };

                $scope.setCookie = function () {
                    $cookieStore.put($scope.keyval.key, $scope.keyval.val);
                };

                $scope.getCookie = function () {
                    $scope.keyval.val = $cookieStore.get($scope.keyval.key);
                };
                $scope.dateOptions = {
                    changeYear: true,
                    changeMonth: true,
                    yearRange: '1900:-0'
                };
                $scope.myDate;
//        $scope.getRemoteCookie=function(){
//            $http.get("../../service/Handler.ashx").
//                then(function(res){
//                    console.info("http complete");
//                    $scope.removeCookie=$cookies.removeCookie +"--"+$cookies.name;
////                    $scope.removeCookie=$cookieStore.get("removeCookie");
//                });
//
//        };
            }]);
});