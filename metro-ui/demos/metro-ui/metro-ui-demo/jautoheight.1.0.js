
/*

version:1.0.0
高度设置
data-height:
10=10px
100%=百分比填充
10*=最小10px,如果还有空间,则填满
auto=自动填满

version:1.0.1
1.排除script的高度;
2.排除容器的margin,padding,border的高度;
3.添加layout的优先级;

*/
(function () {

    function _getPrevHeight(target) {
        var h = 0;
        target.prev().each(function () {
            if ($(this).is("script")) return;
            h += $(this).outerHeight();
        });
        return h;
    }

    function _getNextHeight(target) {
        var h = 0;
        target.next().each(function () {
            if ($(this).is("script")) return;
            h += $(this).outerHeight();
        });
        return h;
    }

    var _layout = function (container) {
        
        
        var _fn = function () {
            //#region 处理高度
            //数字
            var g1 = [];
            //百分比
            var g2 = [];
            //数字*
            var g3 = [];
            //auto
            var g4 = [];
            container.find(">[data-height]").each(function () {
                var attr = $(this).attr("data-height");
                if (/^[0-9]+$/.test(attr)) g1.push($(this));
                if (/^[0-9]+\%$/.test(attr)) g2.push($(this));
                if (/^[0-9]+\*$/.test(attr)) g3.push($(this));
                if (/^auto$/i.test(attr)) g4.push($(this));
            });
            var caculate = function (index, target) {
                var p = target.parent();
                //不包括border,padding-top,padding-bottom,margin-top,margin-bottom
                var contentHeight;
                if (p.is("html")) {
                    var dom_body = $(document.body);
                    contentHeight = $(window).height()
                        - parseInt(dom_body.css("padding-top"))
                        - parseInt(dom_body.css("padding-bottom"))
                        - parseInt(dom_body.css("margin-top"))
                        - parseInt(dom_body.css("margin-bottom"))
                        - parseInt(dom_body.css("border-top-width"))
                        - parseInt(dom_body.css("border-bottom-width"));
                }
                else {
                    contentHeight = p.innerHeight()
                        - parseInt(p.css("padding-top"))
                        - parseInt(p.css("padding-bottom"))
                        - parseInt(p.css("margin-top"))
                        - parseInt(p.css("margin-bottom"))
                        - parseInt(p.css("border-top-width"))
                        - parseInt(p.css("border-bottom-width"));
                }
                var dh = target.attr("data-height");
                //填满
                if (/^auto$/i.test(dh)) {
                    target.height(contentHeight - _getNextHeight(target) - _getPrevHeight(target));
                }
                if (/^[0-9]+$/.test(dh)) {
                    target.height(parseInt(dh));
                }
                //最小是前面的数字,如果当前剩下的空间不够,则设置为数字大小,如果空间有多余,则充满
                if (/^[0-9]+\*$/.test(dh)) {
                    var leftH = contentHeight - _getNextHeight(target) - _getPrevHeight(target);
                    var th = parseInt(dh);
                    if (leftH <= th) {
                        target.height(th);
                    }
                    else {
                        target.height(leftH);
                    }
                }
                if (/^[0-9]+\%$/.test(dh)) {
                    target.height(contentHeight * parseInt(dh) / 100);
                }
            };
            $.each(g1, caculate);
            $.each(g2, caculate);
            $.each(g3, caculate);
            $.each(g4, caculate);
            //#endregion
        };
        _fn();
    };



    $.fn.extend({
        layout: function () {
            _layout($(this));
        }
    });
})();