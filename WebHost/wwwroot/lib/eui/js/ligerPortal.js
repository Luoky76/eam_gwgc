(function ($) {
    $.fn.ligerPortal = function (options) {
        return $.ligerui.run.call(this, "ligerPortal", arguments);
    };
    $.ligerDefaults.Portal = {
        width: null,
        /*行元素：组件允许以纵向方式分割为几块
        每一块(行)允许自定义N个列(column)
        每一列允许自定义N个Panel(最小元素)
        rows:[
        {columns:[
        {
        width : '50%',
        panels : [{width:'100%',content:'内容'},{width:'100%',url:@url1}]
        },{
        width : '50%',
        panels : [{width:'100%',url:@url2}]
        }
        ]}
        ]
        */
        rows: null,
        /* 列元素： 组件将认为只存在一个row(块),
        这一块 允许自定义N个列(column),结构同上
        */
        columns: null,
        url: null,          //portal结构定义URL
        method: 'get',                         //获取数据http方式
        parms: null,                         //提交到服务器的参数
        draggable: false,   //是否允许拖拽
        onLoaded: null       //url模式 加载完事件
    };
    $.ligerDefaults.Portal_rows = {
        width: null,
        height: null
    };
    $.ligerDefaults.Portal_columns = {
        width: null,
        height: null
    };

    $.ligerMethos.Portal = $.ligerMethos.Portal || {};

})(jQuery);