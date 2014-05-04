/**
 * Created by jm96 on 14-4-30.
 */


(function () {

    var config={
        name:"demo",
        debug:true
    };

    function log(msg) {
        if (config.debug)
            console.info(": " + msg);
    }

    angular.module(config.name, []);

    var app;

    function onDocumentRead(){
        app=angular.bootstrap(document, [config.name]);
        log("angular bootstrap");
    }
    angular.element(window).ready(onDocumentRead);
})();
