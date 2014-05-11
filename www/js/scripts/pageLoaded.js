/**
 * Created by Jeanma on 14-5-11.
 */
(function(){

    function menuHeaderClick(){
        $(this).next("ul").toggle();
    }
    function resetContent(){
        var width=$(window).width();
        var height=$(window).height();
        var topHeight=$(".top").height();
        var bodyHeight=height-topHeight;
        $(".left,.right").height(bodyHeight);
    }
    function pageLoaded(){
        resetContent();
        $(".pure-menu-heading").bind("click",menuHeaderClick);
    };
    $(pageLoaded);
    $(window).bind("resize",resetContent);
})();