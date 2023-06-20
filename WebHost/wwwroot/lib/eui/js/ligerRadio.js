(function ($) {
    $.fn.ligerRadio = function () {
        return $.ligerui.run.call(this, "ligerRadio", arguments);
    };

    $.fn.ligerGetRadioManager = function () {
        return $.ligerui.run.call(this, "ligerGetRadioManager", arguments);
    };

    $.ligerDefaults.Radio = {
        label: undefined,
        disabled: false,
        readonly: false, //只读
        initTrigger: false, //初始化触发onChangeValue
        value: null,
        empty: false, //不选中状态是否开启
        onChangeValue: null,//值变化事件
        type: null // l-radio-rect
    };

    $.ligerMethos.Radio = $.ligerMethos.Radio || {};

})(jQuery);