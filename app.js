/**
 * Created by jm96 on 14-4-30.
 */
(function(){
    function initApp(){
        debugger
        angular.module(N.config.name,[]);
        angular.bootstrap(document,[N.config.name]);
        console.info("angular bootstrap");
        //append ng-view
        if($("[ng-view]").length<=0){
            //$("<div ng-view></div>").appendTo($(N.config.viewParent));
            $(N.config.viewParent).append("<div></div>");
            console.info("add a ng-view");
        }
        console.info("ng-view is ready");
    }
    $(initApp);
})();