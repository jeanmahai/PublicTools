//jquery 2.x
//string.format
//array.prototype.each
//array.prototype.find
//object.clone&extend.js
//jtemplate.js

/* 单页html,通过popstate事件异步加载子页面,子页面中可以引用外部资源,如:section,style,script
 * >>>>引用section <!--@section src=***-->
 * >>>>引用style <!--@style src=***-->
 * >>>>引用script <!--@script src=***-->
 *
 * >>>>页面链接和跳转
 *      代码跳转 N.goto(viewUrl);
 *      a 标签跳转 <a href="#viewUrl"></a>
 *
 *
 * */
window["config"] = {
    //info,warn,error
    showLevel: "info"
};
(function () {
    "use strict";
    var config = $.extend(window["config"], {});
    var showLevel = {
        "info": 0,
        "warn": 1,
        "error": 2
    };

    function showInfo(msg) {
        if (showLevel[config.showLevel] <= 0) {
            console.info("%c" + msg, "color:green;");
        }
    }

    function showError(msg) {
        if (showLevel[config.showLevel] <= 2) {
            console.info("%c" + msg, "color:red;");
        }
    }

    function showWarn(msg) {
        if (showLevel[config.showLevel] <= 1) {
            console.info("%c" + msg, "color:#FF8C00;");
        }
    }

    if (!window["N"]) window["N"] = {};
    if (window["page"]) {
        showWarn("window中已经存在page属性");
    }
    var n = window["N"];
    n.getCurrenViewDom = function () {
        return $(document.body);
    };
    n.goto = function (viewUrl) {
        var masterUrl = getMasterUrl(window.location.href);
        var url = String.format(masterUrl + "#{0}", viewUrl);
        showInfo("goto : " + url);
        window.location.href = url;
    };

    var views = [];
    var currentView = null;

    function View() {
        this.section = [];//所有的section
        this.style = [];//所有的样式
        this.script = [];//所有的script,包括内敛的script
        this.html = "";//解析之后的纯html
        this.renderHtml="";//页面在render之后的html
        this.context = null;
        this.url = "";//window.location.href
        this.viewUrl = "";//url # 之后的部分
    }

    View.prototype = {
        setContext: function (obj,emptyContainer,handler) {
            this.context = obj;
            function renderData(){
                var container=$(arguments[1]);
                if(emptyContainer && emptyContainer===true){
                    container.empty();
                }
                var selector=container.attr("data-template");
                var temp=$(selector);
                var dataSource=temp.attr("data-source");
                $(selector).jtemplate(obj[dataSource],handler).appendTo(container);
            };
            //this.renderHtml.find("[data-template]").each(renderData.bind(this));
            N.getCurrenViewDom().find("[data-template]").each(renderData.bind(this));
        },
        loadResource: function () {
            var arr = [];
            this.script.each(function () {
                arr.push(this.load());
            });
            this.style.each(function () {
                arr.push(this.load());
            });
            this.section.each(function () {
                arr.push(this.load());
            });
            return $.when.apply($, arr);
        },
        render: function (appendScript) {
            if (appendScript == "undefined" || appendScript == null) appendScript = true;
            var div = $("<div></div>");
            var html = this.html;
            this.section.each(function () {
                html = html.replace("<!--@section src=" + this.viewUrl + "-->", this.html);
            });
            this.style.each(function () {
                if (this.text == "") return;
                var style = document.createElement("style");
                style.innerHTML = this.text;
                div.append(style);
            });
            div.append(html);
            if (appendScript == true) {
                this.script.each(function () {
                    if (this.text == "") return;
                    var script = document.createElement("script");
                    script.innerHTML = this.text;
                    div.append(script);
                });
            }
            html = div.html();
            this.renderHtml=div;
            return html;
        }
    };

    function Style() {
        this.name = "";
        this.text = "";
    }

    Style.prototype = {
        load: function () {
            var me = this;
            if (this.name != "")
                return loadText(this.name).then(function (res) {
                    me.text = res;
                });
        }
    };

    function Script() {
        Style.call(this);
    }

    Script.prototype = new Style();

    function Section() {
        View.call(this);
        delete this.url;
    }

    Section.prototype = new View();
    Section.prototype.load = function () {
        var me = this;
        if (this.viewUrl != "") {
            return loadText(this.viewUrl).then(function (res) {
                me.html = res;
            });
        }
    };

    function getViewUrl(url) {
        var index = url.indexOf("#");
        if (index < 0) return "";
        return url.substring(index + 1);
    }

    function getMasterUrl(url) {
        var index = url.indexOf("#");
        if (index < 0) return url;
        return url.substring(0, index);
    }

    function loadText(url) {
        return $.ajax({
            url: url
        });
    }

    function analysisHtml(html) {
        //section
        var regTxt = "<!--@section src=(.*)-->";
        var matches = html.match(new RegExp(regTxt, "gi"));

        if (matches) {
            showInfo("section match:", matches.length);
            matches.each(function () {
                var _section = new Section();
                _section.viewUrl = new RegExp(regTxt, "gi").exec(this.valueOf())[1];
                currentView.section.push(_section);
            });
        }
        //style
        regTxt = "<!--@style src=(.*)-->";
        matches = html.match(new RegExp(regTxt, "gi"));

        if (matches) {
            showInfo("style match:", matches.length);
            matches.each(function () {
                var _style = new Style();
                _style.name = new RegExp(regTxt, "gi").exec(this.valueOf())[1];
                currentView.style.push(_style);
            });
        }
        regTxt = "<style [=[a-z\"\\/]*>(.|[\r\n])*</style>";
        matches = html.match(new RegExp(regTxt, "gim"));
        if (matches) {
            matches.each(function () {
                var _style = new Style();
                _style.text = $(this.valueOf()).text().replace(/\n/gi, "");
                currentView.style.push(_style);
            });
            html = html.replace(new RegExp(regTxt, "gi"), "");
            //info(html);
        }
        //script
        regTxt = "<!--@script src=(.*)-->";
        matches = html.match(new RegExp(regTxt, "gi"));

        if (matches) {
            showInfo(String.format("script match : {0}", matches.length));
            matches.each(function () {
                var _script = new Script();
                _script.name = new RegExp(regTxt, "gi").exec(this.valueOf())[1];
                currentView.script.push(_script);
            });
        }
        regTxt = "<script [=[a-z\"\\/]*>(.|[\r\n])*</script>";
        matches = html.match(new RegExp(regTxt, "gim"));
        if (matches) {
            matches.each(function () {
                var _script = new Script();
                _script.text = $(this.valueOf()).text().replace(/\n/gi, "");
                currentView.script.push(_script);
            });
            html = html.replace(new RegExp(regTxt, "gi"), "");
            //info(html);
        }
        return html;
    }

    function compressHtml(html) {
        html = html.replace(/\n/gi, "")
            .replace(/>\s*</gi, "><")
            .replace(/>\s*([@for|\\}])/gi, ">$1")
            .replace(/\{\s*</gi, "{<");
        return html;
    }

    function popstate() {
        showInfo("--> begin page change ...");
        showInfo("remove event : popstate.");
        window.removeEventListener("popstate", popstate);
        var viewUrl = getViewUrl(window.location.href);
        if (viewUrl == "") {
            showWarn("no view url");
            showWarn("navigation to default page");
            window.addEventListener("popstate", popstate);
            showInfo("add event : popstate");
            return;
        }
        currentView = Object.extend({
            url: window.location.href,
            viewUrl: getViewUrl(window.location.href),
            masterUrl: getMasterUrl(window.location.href)
        }, new View());
        window["page"] = currentView;

        showInfo(String.format("begin loading view ... : {0}", viewUrl));
        loadText(viewUrl).then(function (html) {
            html = analysisHtml(html);
            currentView.html = html;
            showInfo("begin loading all resources ...");

            currentView.loadResource().done(function () {
                showInfo("begin append new view ...");
                N.getCurrenViewDom().html(currentView.render());
                showInfo("push new view");
                views.push(currentView);
                window.addEventListener("popstate", popstate);
                showInfo("-->end page changed");
            });
        });
    }

    popstate();
})();