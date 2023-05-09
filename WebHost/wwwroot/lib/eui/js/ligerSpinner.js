(function ($) {
    $.fn.ligerSpinner = function () {
        return $.ligerui.run.call(this, "ligerSpinner", arguments);
    };
    $.fn.ligerGetSpinnerManager = function () {
        return $.ligerui.run.call(this, "ligerGetSpinnerManager", arguments);
    };

    $.ligerDefaults.Spinner = $.extend({}, $.ligerDefaults.TextBox, {
        type: 'float',     //类型 float:浮点数 int:整数 time:时间
        isNegative: true, //是否负数
        precision: 2,   //小数位 type=float时起作用
        step: null,         //每次增加的值
        interval: null,      //间隔，毫秒
        initTrigger: false, //初始化触发onChangeValue
        value: null,
        onChangeValue: null,    //改变值事件
        minValue: null,        //最小值
        maxValue: null,         //最大值
        disabled: false,        //不可用
        readonly: false         //只读
    });
    $.ligerMethos.Spinner = $.ligerMethos.Spinner || {};

})(jQuery);