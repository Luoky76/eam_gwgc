(function ($) {
    $.fn.ligerTextBox = function () {
        return $.ligerui.run.call(this, "ligerTextBox", arguments);
    };

    $.fn.ligerGetTextBoxManager = function () {
        return $.ligerui.run.call(this, "ligerGetTextBoxManager", arguments);
    };

    $.ligerDefaults.TextBox = {
        type: null,//类型
        orginAutocomplete: null,//原生的自动完成
        cancelable: false,//可清空
        icon: "",//图标
        iconType: "",//图标样式
        prefixIcon: false,//图标是否在头部
        onEnter: null,
        onClick: null,
        onChangeValue: null,
        onMouseOver: null,
        onMouseOut: null,
        onBlur: null,
        onFocus: null,
        width: null,
        disabled: false, //不可用
        readonly: false, //只读
        initSelect: false,
        initTrigger: false, //初始化触发onChangeValue
        value: null, //初始化值
        precision: 2, //保留小数位(仅currency时有效)
        nullText: null, //空值提示
        digits: false, //是否限定为数字输入框
        number: false, //是否限定为浮点数格式输入框
        currency: false, //是否显示为货币形式
        trim: true, //文本去两边空格

        xss: true,
        clearPromptChar: false,//清空提示字符 可填入填充的字符串
        promptChar: '_',//提示字符
        rules: {},//附加规则
        mask: '',//掩码
        insertMode: true//插入模式
    };
    $.ligerMethos.TextBox = $.ligerMethos.TextBox || {};

})(jQuery);