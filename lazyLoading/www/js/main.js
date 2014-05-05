/**
 * Created by Jeanma on 14-5-5.
 */
require.config({
    baseUrl:"js/scripts",
    paths: {
        'angular': '../bower_components/angular/angular',
        'angular-route': '../bower_components/angular-route/angular-route',
//        'async': '../lib/requirejs/async',
        'angularAMD': '../bower_components/angularAMD/angularAMD',
//        'ngload': '../../bower_components/angularAMD/ngload'
//        'ui-bootstrap': '../lib/angular-ui-bootstrap/ui-bootstrap-tpls',
//        'prettify': '../lib/google-code-prettify/prettify',

        'HomeController': 'controllers/HomeController'
//        'MapController': 'controller/map_ctrl',
//        'PicturesController': 'controller/pictures_ctrl',
//        'ModulesController': 'controller/modules_ctrl'
    },

    // Add angular modules that does not support AMD out of the box, put it in a shim
    shim: {
        'angularAMD': ['angular'],
        'angular-route': ['angular']
    },

    // kick start application
    deps: ['app']
});