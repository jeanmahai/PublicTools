
/*
高度设置
data-height:10=10px/10*=最小10px,如果还有空间,则填满/auto=自动填满/100%=百分比填充
*/
(function () {

    var _layout = function (container) {
        function _getPrevHeight(target) {
            var h = 0;
            target.prev().each(function () {
                h += $(this).outerHeight();
            });
            return h;
        }
        function _getNextHeight(target) {
            var h = 0;
            target.next().each(function () {
                h += $(this).outerHeight();
            });
            return h;
        }
        var fn = function () {
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
                //不包括border,padding-top,padding-bottom
                var innerHeight;
                if (p.is("html")) {
                    innerHeight = $(window).height();
                }
                else {
                    innerHeight = p.innerHeight() - parseInt(p.css("padding-top")) - parseInt(p.css("padding-bottom"));
                }
                var dh = target.attr("data-height");
                //填满
                if (/^auto$/i.test(dh)) {
                    target.height(innerHeight - _getNextHeight(target) - _getPrevHeight(target));
                }
                if (/^[0-9]+$/.test(dh)) {
                    target.height(parseInt(dh));
                }
                //最小是前面的数字,如果当前剩下的空间不够,则设置为数字大小,如果空间有多余,则充满
                if (/^[0-9]+\*$/.test(dh)) {
                    var leftH = innerHeight - _getNextHeight(target) - _getPrevHeight(target);
                    var th = parseInt(dh);
                    if (leftH <= th) {
                        target.height(th);
                    }
                    else {
                        target.height(leftH);
                    }
                }
                if (/^[0-9]+\%$/.test(dh)) {
                    target.height(innerHeight * parseInt(dh) / 100);
                }
            };
            $.each(g1, caculate);
            $.each(g2, caculate);
            $.each(g3, caculate);
            $.each(g4, caculate);
            console.info(g1.length);
            console.info(g2.length);
            console.info(g3.length);
            console.info(g4.length);
            //#endregion
        };
        fn();
    };

    $.fn.extend({
        layout: function () {
            _layout($(this));
        }
    });
})();