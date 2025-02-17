(function ($) {
    $.fn.ligerCheckBox = function (options) {
        return $.ligerui.run.call(this, "ligerCheckBox", arguments);
    };
    $.fn.ligerGetCheckBoxManager = function () {
        return $.ligerui.run.call(this, "ligerGetCheckBoxManager", arguments);
    };
    $.ligerDefaults.CheckBox = {
        label: undefined,
        disabled: false,
        readonly: false, //只读
        initTrigger: false, //初始化触发onChangeValue
        value: null,
        onChangeValue: null,//值变化事件
        type: "orgin" //checkbox样式 rect switch orgin
    };

    $.ligerMethos.CheckBox = $.ligerMethos.CheckBox || {};

})(jQuery);