(function ($) {
    //气泡,可以在制定位置显示
    $.ligerTip = function (p) {
        return $.ligerui.run.call(null, "ligerTip", arguments);
    };

    //在指定Dom Element右侧显示气泡
    //target：将ligerui对象ID附加上
    $.fn.ligerTip;
    //关闭指定在Dom Element(附加了ligerui对象ID,属性名"ligeruitipid")显示的气泡
    $.fn.ligerHideTip;

    $.fn.ligerGetTipManager = function () {
        return $.ligerui.get(this);
    };

    $.ligerDefaults = $.ligerDefaults || {};

    //隐藏气泡
    $.ligerDefaults.HideTip = {};

    //气泡
    $.ligerDefaults.Tip = {
        content: null,
        callback: null,
        width: 150,
        height: null,
        x: 0,
        y: 0,
        appendIdTo: null,       //保存ID到那一个对象(jQuery)(待移除)
        target: null,
        auto: null,             //是否自动模式，如果是，那么：鼠标经过时显示，移开时关闭,并且当content为空时自动读取attr[title]
        removeTitle: true        //自动模式时，默认是否移除掉title
    };

    //在指定Dom Element右侧显示气泡,通过$.fn.ligerTip调用
    $.ligerDefaults.ElementTip = {
        distanceX: 8,
        distanceY: -10,
        auto: null,
        removeTitle: true
    };

    $.ligerMethos.Tip = $.ligerMethos.Tip || {};

})(jQuery);