(function ($) {
    $.fn.ligerDateEditor = function () {
        return $.ligerui.run.call(this, "ligerDateEditor", arguments);
    };

    $.fn.ligerGetDateEditorManager = function () {
        return $.ligerui.run.call(this, "ligerGetDateEditorManager", arguments);
    };

    $.ligerDefaults.DateEditor = $.extend({}, $.ligerDefaults.TextBox, {
        initValue: "",//初始值
        format: "yyyy-MM-dd",//格式 yyyy-MM-dd hh:mm:ss
        nullText: null,//空值提示
        valueType: "date",//返回值类型 date string
        range: false,//时间范围选择
        split: " → ",//时间范围分隔符
        width: null,//宽度
        showTime: false,//是否显示时间，同format联动
        initTrigger: false,//初始化触发onChangeValue
        value: null,//值
        onChangeValue: false,//值变更事件 function(input,value,g)
        absolute: true,//选择框是否在附加到body,并绝对定位
        focusToggle: null,//获取焦点是否展开下拉，默认在grid时不展开，其他展开
        alwayShowInTop: false,//下拉框是否一直显示在上方
        alwayShowInDown: false,//下拉框是否一直显示在上方
        yDiff: 0,//下拉框位置y坐标调整
        cancelable: true,//可清空
        disabled: false, //不可用
        readonly: false, //只读
        minDate: null,//可选择的最小日期
        maxDate: null//可选择的最大日期
    });
    $.ligerDefaults.DateEditorString = {
        dayMessage: ["一", "二", "三", "四", "五", "六", "日"],
        monthMessage: ["1月", "2月", "3月", "4月", "5月", "6月", "7月", "8月", "9月", "10月", "11月", "12月"],
        todayMessage: "今天",
        nowMessage: "此刻",
        confirmMessage: "确定",
        closeMessage: "关闭"
    };
    $.ligerMethos.DateEditor = $.ligerMethos.DateEditor || {};

})(jQuery);