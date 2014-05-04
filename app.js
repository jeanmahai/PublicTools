/**
 * Created by jm96 on 14-4-30.
 */
(function () {
    function log(msg) {
        if (N.config.debug)
            console.info(": " + msg);
    }

    function initApp() {
        angular.module(N.config.name, []);
        angular.bootstrap(document, [N.config.name]);
        log("angular bootstrap");
        //append ng-view
        var ngView = $("[ng-view]");
        if (ngView.length <= 0) {
            $("<div ng-view></div>").appendTo(document.body);
            log("add a ng-view");
        }
        if (ngView.length > 1) {
            throw new Error("exist multiply ng-view");
        }
        log("ng-view is ready");
    }

    $(initApp);
})();