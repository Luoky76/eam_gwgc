(function ($) {
    $.fn.ligerButton = function (options) {
        return $.ligerui.run.call(this, "ligerButton", arguments);
    };
    $.fn.ligerGetButtonManager = function () {
        return $.ligerui.run.call(this, "ligerGetButtonManager", arguments);
    };

    $.ligerDefaults.Button = {
        debounce: 300,//防抖
        width: null,//宽度
        type: "",//类型 primary circle ghost large
        text: '',//显示文本
        tip: "",//提示信息
        disabled: false,//不可用
        onClick: null,//点击事件
        icon: null//图标 "fa fa-search"
    };

    $.ligerMethos.Button = $.ligerMethos.Button || {};

})(jQuery);