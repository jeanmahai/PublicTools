/**
 * Created by Jeanma on 14-5-5.
 */
(function () {
    function getShortDateString() {
        var date = new Date();
        //cache 1 hour
        //return date.getFullYear()+date.getMonth()+date.getDate()+date.getHours();

        //no cache
        return Math.random();
    }

    function loadCss(url) {
        var link = document.createElement("link");
        link.type = "text/css";
        link.rel = "stylesheet";
        link.href = url;
        document.getElementsByTagName("head")[0].appendChild(link);
    }

    require.config({
        baseUrl: "js/scripts/controllers",
        paths: {
            'angular': '../../bower_components/angular/angular.min',
            'angular-route': '../../bower_components/angular-route/angular-route.min',
            'angularAMD': '../../bower_components/angularAMD/angularAMD',
            'angular-cookies': '../../bower_components/angular-cookies/angular-cookies.min',
            "N": "../N",
            'jquery': '../../bower_components/jquery/jquery.min',
            'ng-grid': "../../bower_components/ng-grid/ng-grid-2.0.11.min",
            'app': "../app",
            'jquery-ui': "../../bower_components/jquery-ui/ui/jquery-ui",
            "angular-date": "../../bower_components/angular-ui-date/src/date",
            "jquery-ui-datepicker-zh-cn": "../../bower_components/jquery-ui/ui/i18n/jquery.ui.datepicker-zh-CN"
        },

        // Add angular modules that does not support AMD out of the box, put it in a shim
        shim: {
            'angular': {
                deps:["jquery"],
                init:function(){
                    //loadCss("js/bower_components/pure/pure-min.css");
                }
            },
            'angularAMD': ['angular'],
            'angular-route': ['angular'],
            "ng-grid": {
                deps: ["angular","jquery"],
                init: function () {
                    loadCss("js/bower_components/ng-grid/ng-grid.min.css");
                }
            },
            "angular-cookies": ["angular"],
            "N": {
                deps: ["angular"],
                init: function () {
                    loadCss("css/N.css");
                }
            },
            "jquery-ui":["jquery"],
            "angular-date": {
                deps: ["angular","jquery","jquery-ui"],
                init: function (a,b,c) {
                    $.datepicker.regional['zh-CN'] = {
                        closeText: '关闭',
                        prevText: '&#x3C;上月',
                        nextText: '下月&#x3E;',
                        currentText: '今天',
                        monthNames: ['一月','二月','三月','四月','五月','六月',
                            '七月','八月','九月','十月','十一月','十二月'],
                        monthNamesShort: ['一月','二月','三月','四月','五月','六月',
                            '七月','八月','九月','十月','十一月','十二月'],
                        dayNames: ['星期日','星期一','星期二','星期三','星期四','星期五','星期六'],
                        dayNamesShort: ['周日','周一','周二','周三','周四','周五','周六'],
                        dayNamesMin: ['日','一','二','三','四','五','六'],
                        weekHeader: '周',
                        dateFormat: 'yy-mm-dd',
                        firstDay: 1,
                        isRTL: false,
                        showMonthAfterYear: true,
                        yearSuffix: ''};
                    $.datepicker.setDefaults($.datepicker.regional['zh-CN']);
                    loadCss("js/bower_components/jquery-ui/themes/ui-darkness/jquery-ui.min.css");
                }
            }
        },

        // kick start application
        deps: ['app']
        //set javascript no cache
        , urlArgs: "_=" + getShortDateString()
    });
})();
