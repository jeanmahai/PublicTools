/**
 * Created by jm96 on 14-5-9.
 * appConfig 整个app的配置
 * appRouteUrl url route 配置项
 */
(function(){
    //config
    window["appConfig"]={
        showLoading:true,
        loadingDom:document.getElementById("divLoading"),
        angularModualJS:["angularAMD"
            , "angular-route"
            //, "ng-grid"
            , "angular-cookies"
            , "angular-date"
            , "N"],
        angularModualNames:["ngRoute"//
            // , "ngGrid"
            , "ngCookies"
            , "NProvider"
            , "ui.date"]
    };
    //url route
    window["appRouteUrl"]={
        home:{
            routeUrl:"/home",
            templateUrl:"views/home.html",
            controller:"homeController"
        },
        dataGrid1:{
            routeUrl:"/pure-table",
            templateUrl:"views/dataGrid1.html",
            controller:"dataGrid1Controller"
        },
        demo:{
            routeUrl:"/demo",
            templateUrl:"views/dynamicLoadControllerAndView.html",
            controller:"DynamicController"
        },
        datepicker:{
            routeUrl:"/datepicker",
            templateUrl:"views/datepicker.html",
            controller:"datepickerController"
        },
        //默认跳转页面
        otherwise:{
            redirectTo:"/home"
        }
    };
})();
