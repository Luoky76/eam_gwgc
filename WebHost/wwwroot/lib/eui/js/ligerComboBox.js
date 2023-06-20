(function ($) {
    $.fn.ligerComboBox = function (options) {
        return $.ligerui.run.call(this, "ligerComboBox", arguments);
    };

    $.fn.ligerGetComboBoxManager = function () {
        return $.ligerui.run.call(this, "ligerGetComboBoxManager", arguments);
    };

    $.ligerDefaults.ComboBox = $.extend({}, $.ligerDefaults.TextBox, {
        width: null,//宽度
        height: null,//高度
        selectBoxWidth: null,//下拉宽度
        selectBoxHeight: null,//下拉高度
        data: null, //数据源
        valueField: 'ID',//实际值
        textField: 'TEXT',//显示值
        cancelable: true,//取消选择
        autocomplete: true,  //自动完成
        autocompleteAllowEmpty: false, //是否允许空值搜索
        pingyinable: true, //可输入拼音首字母快速检索
        hightLight: true,//自动完成是否匹配字符高亮显示
        canDifferent: false,//是否直接可以不是下拉值
        isMultiSelect: false,   //是否多选
        split: ";",//多选分隔符
        nullText: null,//空值提示
        url: null,  //数据源URL(需返回JSON)
        ajaxType: 'post',//数据请求方式
        async: undefined,//异步请求
        parms: null, //url参数
        urlSearch: false, //默认启用URL的search
        keydbname: null, //用于key查询的dbname(url和grid)
        op: null,//查询条件 默认like
        ajaxOptions: null,//ajax扩展属性
        ajaxBeforeSend: null,//开始请求事件
        ajaxComplete: null,//完成请求事件
        ajaxContentType: null,//请求内容类型
        dataGetter: null,//请求后数据处理
        dataParmName: null,//请求后数据处理
        resize: false,//是否调整大小
        value: null,//初始值
        initTrigger: false,//初始化时是否触发选择事件
        absolute: true,//选择框是否在附加到body,并绝对定位
        textFieldID: null,//显示控件ID
        valueFieldID: null,//值控件ID
        readonly: false,//只读
        disabled: false,//不可用
        selectBoxRender: null,       //自定义selectbox的内容
        triggerToLoad: false, //是否在点击下拉按钮时加载
        triggerIcon: null, //下拉按钮
        addRowButton: '新增',           //新增按钮
        addRowButtonClick: null,        //新增事件
        css: null,//控件css样式
        grid: null,//下拉grid
        delayLoadGrid: true,       //是否在按下显示下拉框的时候才 加载 grid
        hideGridOnLoseFocus: false,
        condition: null,       //列表条件搜索 参数同 ligerForm
        conditionSearchClick: null,      //下拉框表格搜索按钮自定义函数
        tree: null,//下拉树
        treeLeafOnly: true,   //是否只选择叶子
        setTextBySource: true,//设置文本框值时是否从数据源中加载
        columns: null,//下拉表格
        renderItem: null,//自定义展示
        isRowReadOnly: null,//选项是否只读的判定函数
        rowClsRender: null,//选项行 class name 自定义函数
        alwayShowInTop: false,      //下拉框是否一直显示在上方
        alwayShowInDown: false,      //下拉框是否一直显示在上方
        selectBoxPosYDiff: 0, //下拉框位置y坐标调整
        emptyText: null,//空行
        emptyValue: null,//空行值
        slide: true,//是否以动画的形式显示
        render: null,//文本框显示html函数

        itemHeight: 29,//行高
        headerHeight: 29.5,//标题高
        onChangeValue: null, //值变化事件
        onBeforeSelect: false, //选择前事件
        onAfterShowData: null,
        onSelected: null, //选择值事件
        onStartResize: null,//调整大小之前
        onEndResize: null,//调整大小之后
        onBeforeSetData: null,//ajax展示数据之前
        onSuccess: null,//ajax获取数据完成后
        onError: null,//url数据展示出错后
        onBeforeOpen: null,//打开下拉框前事件，可以通过return false来阻止继续操作，利用这个参数可以用来调用其他函数，比如打开一个新窗口来选择值
        onButtonClick: null//右侧图标按钮事件，可以通过return false来阻止继续操作，利用这个参数可以用来调用其他函数，比如打开一个新窗口来选择值
    });
    $.ligerDefaults.ComboBoxString = {
        Search: "搜索"
    };
    //扩展方法
    $.ligerMethos.ComboBox = $.ligerMethos.ComboBox || {};

})(jQuery);