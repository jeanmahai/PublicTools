//jquery 2.x
//string.format
//array.prototype.each
//array.prototype.find
(function () {
    "use strict";
    if (!window["N"]) window["N"] = {};
    if (window["page"]) {
        console.warn("window中已经存在page属性");
    }
    var n = window["N"];
    n.getCurrenView = function () {
        return $(document.body);
    };

    var views = [];
    var currentView = null;
    var keyword=["@if","@for","@foreach","@section","@style","@script"];

    function getNewView() {
        return views[views.length - 1];
    }

    function View() {
        this.section = [];//所有的section
        this.style = [];//所有的样式
        this.script = [];//所有的script,包括内敛的script
        this.html = "";//解析之后的纯html
        this.context = null;
        this.url = "";//window.location.href
        this.viewUrl = "";//url # 之后的部分
    }

    View.prototype = {
        setContext: function (obj) {
            this.context=obj;
        },
        getValue:function(w){
            w= w.replace(/@context/i,"this.context");
            return eval(w);
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
                    //html.append(script);
                    div.append(script);
                });
            }
            html = div.html();
            div = null;
            return html;
        },
        bindData:function(html){
            html=compressHtml(html);
            var result=[];
            var temp="";
            var i=0;
            var len=html.length;
            var item;
            var status=0;//0 文本 1 属性 2 语句
            for(;i<len;){
                item=html[i];
                if(item=="@"){
                    if(html[i+1]=="@"){
                        temp+="@";
                        i++;
                    }
                    else{
                        result.push(temp);
                        temp="";
                        temp=item;
                        i++
                    }
                }
                else{
                    temp+=item;
                    i++;
                }
                if(this.isKeyword(temp)){
                    result.push(temp);
                    temp="";
                }
                //if(new RegExp("^@[a-z_][_a-z0-9]*\."))
            }
            return result;
        },
        isKeyword:function(kw){
            var result=keyword.find(function(val){
                if(val==kw) return true;
                return false;
            });
            return result.length>0;
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

    function extend(o1, o2) {
        for (var p in o1) o2[p] = o1[p];
        return o2;
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
        console.info("section match:", matches.length);
        if (matches)
            matches.each(function () {
                var _section = new Section();
                _section.viewUrl = new RegExp(regTxt, "gi").exec(this.valueOf())[1];
                currentView.section.push(_section);
            });
        //style
        regTxt = "<!--@style src=(.*)-->";
        matches = html.match(new RegExp(regTxt, "gi"));
        console.info("style match:", matches.length);
        if (matches)
            matches.each(function () {
                var _style = new Style();
                _style.name = new RegExp(regTxt, "gi").exec(this.valueOf())[1];
                currentView.style.push(_style);
            });
        regTxt = "<style [=[a-z\"\\/]*>(.|[\r\n])*</style>";
        matches = html.match(new RegExp(regTxt, "gim"));
        if (matches) {
            matches.each(function () {
                var _style = new Style();
                _style.text = $(this.valueOf()).text().replace(/\n/gi, "");
                currentView.style.push(_style);
            });
            html = html.replace(new RegExp(regTxt, "gi"), "");
            //console.info(html);
        }
        //script
        regTxt = "<!--@script src=(.*)-->";
        matches = html.match(new RegExp(regTxt, "gi"));
        console.info("script match:", matches.length);
        if (matches)
            matches.each(function () {
                var _script = new Script();
                _script.name = new RegExp(regTxt, "gi").exec(this.valueOf())[1];
                currentView.script.push(_script);
            });
        regTxt = "<script [=[a-z\"\\/]*>(.|[\r\n])*</script>";
        matches = html.match(new RegExp(regTxt, "gim"));
        if (matches) {
            matches.each(function () {
                var _script = new Script();
                _script.text = $(this.valueOf()).text().replace(/\n/gi, "");
                currentView.script.push(_script);
            });
            html = html.replace(new RegExp(regTxt, "gi"), "");
            //console.info(html);
        }
        //context
//        regTxt = "<!--@context .* -->";
//        matches = html.match(new RegExp(regTxt, "gi"));
//        console.info("context match:", matches.length);
//        matches.each(function () {
//            console.info("context:" + this.valueOf());
//        });
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
        window.removeEventListener("popstate", popstate);
        console.info("event:popstate");
        console.info(String.format("real page url:{0}", getViewUrl(window.location.href)));
        var viewUrl = getViewUrl(window.location.href);
        if (viewUrl == "") {
            console.warn("no real url");
            console.warn("navigation to default page");
            window.addEventListener("popstate", popstate);
            return;
        }
        currentView = extend({
            url: window.location.href,
            viewUrl: getViewUrl(window.location.href),
            masterUrl: getMasterUrl(window.location.href)
        }, new View());

        console.info("loading page");
        loadText(currentView.viewUrl).then(function (html) {
            //console.info(html);
            //var minHtml = compressHtml(html);
            //console.info(minHtml);
            html = analysisHtml(html);
            currentView.html = html;
            N.getCurrenView().append(html);
            currentView.loadResource().done(function () {
                console.info("all resource loaded");
                var boundHtml=currentView.bindData(currentView.html);
                console.info(boundHtml);
                N.getCurrenView().html(currentView.render());
            });
            //tempView.appendScript();
            window["page"] = currentView;
        });
        /*
         $.load(_view.viewUrl,function(html){
         analysisHtml(html);
         console.info(views);
         });
         */
        window.addEventListener("popstate", popstate);
    }

    popstate();
})();