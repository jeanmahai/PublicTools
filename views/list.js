/**
 * Created by jm96 on 14-5-5.
 */
console.info("lazy loading script");
angular.module('app').controllerProvider.resgister('SomeLazyController', function($scope)
{
    $scope.key = '...';
});