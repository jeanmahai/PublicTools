/*
 基于jquery

 通过设置attribute[data-template=true]来标示模板容器;
 模板使用data-template-item=true,如:
 <div data-template="true">
 <ul>
 <script type="text/html" data-template-item="true">
 <li>姓名:#[name]</li>
 </script>
 </ul>
 </div>
 data-bind-model 设置绑定模式,目前只有一个值"dynamicobject",模版如下:
 <div id="tm" data-template="true">
 <ul>
 <script type="text/html" data-template-item="true" data-bind-model="dynamicobject">
 <li>#[{#pro}]--#[{pro}]</li>
 </script>
 </ul>
 </div>
 ***************************************************************************
 **在需要使用语法#[{#pro}]/#[{pro}]时,一定要设置data-bind-model="dynamicobject"**
 ***************************************************************************
 #[{#pro}]属性名称,#[{pro}]属性对应的值

 使用方法:
 $(模版容器).setDataSource(数据源,处理函数).dataBind();
 如:
 $("#tm").setDataSource(dataSource,{
 fn:function(args){
 return "";
 }
 }).dataBind();



 特性:
 data-template:true|false
 data-template-item:true|false
 data-bind-model:dynamicobject

 语法:
 #[{#pro}]
 #[{pro}]
 #[Name]
 #[#item.fn(Name)]
 #[#item.fn(Name,Sex)]
 #[#item.fn(Name,Sex,globalValue)],如果item中的属性名称和全局变量名重复,则优先使用item下的值
 #[#item.fn({#pro})]
 #[#item.fn({pro})]

 关键字:
 {#pro}
 {pro}
 {item}
 #item

 **************************************************
 共享模版
 不需要设置data-template和data-template-item,但是data-bind-model根据需要添加;
 模版可以定义在任何地方,建议最好定义在header中;
 调用格式:
 $(模版).jtemplate(数据源,回调).appendTo($(容器));

 ==============
 v 1.0.1
 模版容器不需要设置data-template=true;
 容器的模版设置方式由data-item=true变为data-template-item=true;
 */
(function () {

    var key = {
        dataSource: "datasource",
        handler: "handler",
        dataTemplate: "data-template",
        dataItem: "data-template-item",
        dataBindModel: "data-bind-model"
    };
    var bindModel = {
        objDynamic: "dynamicobject"
    };

    function bindArray(ds, item, hdl) {
        var i, html = "";
        for (i = 0; i < ds.length; i++) {
            if (!ds[i].push) {
                html += bindObject(ds[i], item, hdl);
            }
            else {
                bindArray(ds[i], item, hdl);
            }
        }
        return html;
    }

    function analysis(text, dataItem, hdl, pro) {
        var syntax = [];
        var temp = "";
        var customSyntaxStart = false;
        var i, k, c;
        for (i = 0; i < text.length; ) {

            if (text[i] == "#" && text[i + 1] == "[") {
                syntax.push(temp);
                temp = "#[";
                i += 2;
                customSyntaxStart = true;
            }
            else if (text[i] == "]" && customSyntaxStart) {
                temp += "]";
                //一个自定义语法接触可以进行处理
                //获得语法内容
                c = temp.match(/^\#\[(.*)\]$/)[1];
                if (/^[_a-z][_a-z0-9]*$/i.test(c)) {
                    //field
                    temp = dataItem[c];
                }
                else if (/^\{pro\}$/i.test(c)) {
                    //dynamic field
                    temp = dataItem[pro];
                }
                else if (/^\{\#pro\}$/i.test(c)) {
                    //
                    temp = pro;
                }
                else if (/^\{item\}$/i.test(c)) {
                    //{item}
                    temp = dataItem;
                }
                else {
                    //#item.fn
                    var mats = c.match(/\#item\.([_a-z][_a-z0-9]*)\((.*)\)/i);

                    var fnname = mats[1];
                    //处理参数
                    var params = mats[2].match(/([_a-z][_a-z0-9]*)|(\{item\})|(\{pro\})|(\{\#pro\})/gi);
                    var args = {};
                    for (k = 0; k < params.length; k++) {
                        //field
                        if (/^[_a-z][_a-z0-9]*$/i.test(params[k])) {
                            if (dataItem[params[k]]) {
                                args[params[k]] = dataItem[params[k]];
                            }
                            else if (params[k]) {
                                try {
                                    args[params[k]] = eval(params[k]);
                                }
                                catch (ex) {
                                    args[params[k]] = params[k];
                                }
                            }
                            else {
                                args[params[k]] = "";
                            }
                        }
                        //{pro}
                        else if (/^\{pro\}$/i.test(params[k])) {

                            args["pro"] = dataItem[pro];
                        }
                        //{#pro}
                        else if (/^\{\#pro\}$/i.test(params[k])) {
                            args["_pro"] = pro;
                        }
                        //{item}
                        else if (/^\{item\}$/i.test(params[k])) {
                            args["item"] = dataItem;
                        }
                        else {
                            args[params[k]] = "";
                        }
                    }
                    temp = hdl[fnname](args);
                }
                //-------------
                customSyntaxStart = false;
                syntax.push(temp);
                i += 1;
                temp = "";
            }
            else {
                temp += text[i];
                i += 1;
            }
        }
        syntax.push(temp);
        return syntax.join("");
    }

    function bindObject(ds, item, hdl) {
        var text;
        var model = item.attr(key.dataBindModel);
        var i, html = "";
        if (model == bindModel.objDynamic) {
            for (i in ds) {
                text = item.text();
                html += analysis($.trim(text), ds, hdl, i);
            }
        }
        else {
            text = item.text();
            html = analysis($.trim(text), ds, hdl);
        }
        return html;
    }

    $.fn.extend({
        setDataSource: function (ds, handler) {
            //if ($(this).attr(key.dataTemplate) != "true") return null;
            $(this).data(key.dataSource, ds);
            $(this).data(key.handler, handler);
            return $(this);
        },
        dataBind: function () {
            //if ($(this).attr(key.dataTemplate) != "true") return;

            var item = $(this).find("[" + key.dataItem + "=true]");

            if (item.length <= 0) return;
            item = item.eq(0);

            var ds = $(this).data(key.dataSource);
            if (!ds) return;

            var hdl = $(this).data(key.handler);
            if (!hdl) hdl = {};

            var html = "";
            if (ds.push) {
                html = bindArray(ds, item, hdl);
            }
            else {
                html = bindObject(ds, item, hdl);
            }

            var container = item.parent();
            container.append(html);
        },
        emptyTmpl: function () {
            $(this).find("[data-template-item=true]").parent().find(":not([data-template-item=true])").remove();
        },
        jtemplate: function (ds, handler) {
            var item = $(this);
            var html;
            if (ds.push) {
                html = bindArray(ds, item, handler);
            }
            else {
                html = bindObject(ds, item, handler);
            }
            return $(html);
        }
    });
})();