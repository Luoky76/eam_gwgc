(function ($) {
    $.fn.ligerRadioList = function (options) {
        return $.ligerui.run.call(this, "ligerRadioList", arguments);
    };

    $.ligerDefaults.RadioList = {
        rowSize: 3,            //每行显示元素数
        valueField: 'ID',       //值成员
        textField: 'TEXT',      //显示成员
        valueFieldID: null,      //隐藏域
        name: null,            //表单名
        data: null,             //数据
        parms: null,            //ajax提交表单
        url: null,              //数据源URL(需返回JSON)
        async: true,            //是否异步
        urlParms: null,         //url带参数
        ajaxOptions: null,       //ajax扩展属性
        ajaxContentType: null,
        ajaxType: 'post',
        onSuccess: null,
        onError: null,
        onSelect: null,
        css: null,               //附加css
        initTrigger: false, //初始化触发onChangeValue
        value: null,            //值
        valueFieldCssClass: null,
        type: "orgin", //checkbox样式 rect border orgin
        empty: false, //不选中状态是否开启
        onChangeValue: null//值变化事件
    };

    //扩展方法
    $.ligerMethos.RadioList = $.ligerMethos.RadioList || {};

})(jQuery);