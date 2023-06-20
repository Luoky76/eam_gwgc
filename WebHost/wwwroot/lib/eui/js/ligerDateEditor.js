(function ($) {
    $.fn.ligerDateEditor = function () {
        return $.ligerui.run.call(this, "ligerDateEditor", arguments);
    };

    $.fn.ligerGetDateEditorManager = function () {
        return $.ligerui.run.call(this, "ligerGetDateEditorManager", arguments);
    };

    $.ligerDefaults.DateEditor = $.extend({}, $.ligerDefaults.TextBox, {
        initValue: "",
        format: "yyyy-MM-dd",
        nullText: null,
        valueType: "date",
        width: null,
        showTime: false,
        initTrigger: false, //初始化触发onChangeValue
        value: null,
        onChangeValue: false,
        absolute: true,  //选择框是否在附加到body,并绝对定位
        cancelable: true,//可清空
        disabled: false, //不可用
        readonly: false, //只读
        minDate: null,
        maxDate: null
    });
    $.ligerDefaults.DateEditorString = {
        dayMessage: ["日", "一", "二", "三", "四", "五", "六"],
        monthMessage: ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"],
        todayMessage: "今天",
        confirmMessage: "确定",
        closeMessage: "关闭"
    };
    $.ligerMethos.DateEditor = $.ligerMethos.DateEditor || {};

})(jQuery);