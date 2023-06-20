(function ($) {
    var l = $.ligerui;
    $.fn.ligerDrag = function (options) {
        return l.run.call(this, "ligerDrag", arguments,
            {
                idAttrName: 'ligeruidragid', hasElement: false, propertyToElemnt: 'target'
            }
        );
    };
    $.fn.ligerGetDragManager = function () {
        return l.run.call(this, "ligerGetDragManager", arguments,
            {
                idAttrName: 'ligeruidragid', hasElement: false, propertyToElemnt: 'target'
            });
    };
    $.ligerDefaults.Drag = {
        onStartDrag: false,
        onDrag: false,
        onStopDrag: false,
        handler: null,
        //鼠标按下再弹起，如果中间的间隔小于[dragDelay]毫秒，那么认为是点击，不会进行拖拽操作
        clickDelay: 100,
        //代理 拖动时的主体,可以是'clone'或者是函数,放回jQuery 对象
        proxy: true,
        revert: false,
        animate: true,
        onRevert: null,
        onEndRevert: null,
        //接收区域 jQuery对象或者jQuery选择字符
        receive: null,
        //进入区域
        onDragEnter: null,
        //在区域移动
        onDragOver: null,
        //离开区域
        onDragLeave: null,
        //在区域释放
        onDrop: null,
        disabled: false,
        proxyX: null,     //代理相对鼠标指针的位置,如果不设置则对应target的left
        proxyY: null,
        mask: false     //拖动遇到iframe会出问题 可加入遮罩处理
    };

})(jQuery);