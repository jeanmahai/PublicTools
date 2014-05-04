/**
 * Created by jm96 on 14-5-4.
 */
(function(){
    "use strict";

    angular.module("customHttp",["ng"])
        .provider("_http",_httpProvider);

    function _httpProvider(){
        this.$get=["$http",function($http){
            var that={};
            that.test=function(){
                console.info("it's a provider test.");
            };
            return that;
        }];
    };
})();