(function ($) {
    $.fn.ligerListBox = function (options) {
        return $.ligerui.run.call(this, "ligerListBox", arguments);
    };

    $.ligerDefaults.ListBox = {
        isMultiSelect: false,   //是否多选
        isShowCheckBox: true,  //是否选择复选框
        title: "",            //是否有标题
        filter: true,     //是否过滤
        onFilterIconClick: null,//过滤按钮点击
        columns: null,          //表格状态
        width: null,            //宽度
        height: null,           //高度
        onSelect: false,        //选择前事件
        onSelected: null,       //选择值事件
        valueField: 'ID',       //值成员
        textField: 'TEXT',      //显示成员
        valueFieldID: null,     //值 隐藏域 表单名
        split: ";",             //分隔符
        searchSplit: null,         //查询分隔符
        data: null,             //数据
        parms: null,            //ajax提交表单
        url: null,              //数据源URL(需返回JSON)
        urlParms: null,                     //url带参数
        ajaxOptions: null,       //ajax扩展属性
        ajaxContentType: null,
        ajaxType: 'post',
        onSuccess: null,
        onError: null,
        render: null,            //显示html自定义函数
        css: null,               //附加css
        value: null,            //值
        valueFieldCssClass: null,
        onChangeValue: null//值变化事件
    };

    //扩展方法
    $.ligerMethos.ListBox = $.ligerMethos.ListBox || {};

})(jQuery);