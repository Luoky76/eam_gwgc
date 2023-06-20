(function ($) {
    $.fn.ligerTree = function (options) {
        return $.ligerui.run.call(this, "ligerTree", arguments);
    };

    $.fn.ligerGetTreeManager = function () {
        return $.ligerui.run.call(this, "ligerGetTreeManager", arguments);
    };

    $.ligerDefaults.Tree = {
        url: null,
        urlParms: null,                     //url带参数
        data: null,         //数据
        checkbox: true,  //是否复选框
        autoCheckboxEven: true,  //复选框联动
        enabledCompleteCheckbox: true,     //是否启用半选择
        parentIcon: 'fa-folder', //父节点图标
        parentIconOpen: "fa-folder-open", //父节点图标打开状态
        childIcon: 'fa-file', //子节点图标
        textFieldName: 'TEXT', //文本字段名
        attribute: ['id', 'url'], //预加载的属性名
        treeLine: true,            //是否显示连接线
        nodeWidth: null, //节点宽度
        statusName: '__status', //状态名
        isLeaf: null,              //是否子节点的判断函数
        single: false,               //是否单选
        needCancel: true, 		//已选的是否需要取消操作
        onBeforeExpand: function () { }, //展开前事件
        onContextmenu: function () { }, //右击事件
        onExpand: function () { }, //展开事件
        onBeforeCollapse: function () { }, //收缩前事件
        onCollapse: function () { }, //收缩事件
        onBeforeSelect: function () { }, //选择前事件
        onSelect: function () { }, //选择事件
        onBeforeCancelSelect: function () { }, //取消选择前事件
        onCancelselect: function () { }, //取消选择事件
        onCheck: function () { }, //选择事件
        onSuccess: function () { }, //加载成功事件
        onError: function () { }, //加载错误事件
        onClick: function () { }, //点击事件
        idFieldName: 'ID', //id字段
        parentIDFieldName: null, //父节点字段
        topParentIDValue: 0, //顶级节点
        onBeforeAppend: function () { },        //加载数据前事件，可以通过return false取消操作
        onAppend: function () { },             //加载数据时事件，对数据进行预处理以后
        onAfterAppend: function () { },         //加载数据完事件
        slide: true,          //是否以动画的形式显示
        iconFieldName: null,
        nodeDraggable: false,             //是否允许拖拽
        nodeDraggingRender: null,
        btnClickToToggleOnly: true,     //是否点击展开/收缩 按钮时才有效
        ajaxOptions: null,       //ajax扩展属性
        ajaxType: 'post',
        ajaxContentType: null,
        render: null,               //自定义函数
        selectable: null,           //可选择判断函数
        /*
        是否展开
        1,可以是true/false
        2,也可以是数字(层次)N 代表第1层到第N层都是展开的，其他收缩
        3,或者是判断函数 函数参数e(data,level) 返回true/false

        优先级没有节点数据的isexpand属性高,并没有delay属性高
        */
        isExpand: null,
        /*
        是否延迟加载
        1,可以是true/false
        2,也可以是数字(层次)N 代表第N层延迟加载
        3,或者是字符串(Url) 加载数据的远程地址
        4,如果是数组,代表这些层都延迟加载,如[1,2]代表第1、2层延迟加载
        5,再是函数(运行时动态获取延迟加载参数) 函数参数e(data,level),返回true/false或者{url:...,parms:...}

        优先级没有节点数据的delay属性高
        */
        delay: null,

        //id字段
        idField: null,
        //parent id字段，可用于线性数据转换为tree数据
        parentIDField: null,
        iconClsFieldName: null
    };

})(jQuery);