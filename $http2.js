/**
 * Created by jm96 on 14-5-4.
 */
(function(){
    "use strict";

    angular.module("http2Provider",[])
        .provider("$http2",function(){
            this.$get=["$http",function($http){
                return {
                    get:function(ops){
                        return $http.get(ops);
                    }
                };
            }];
        });

})();