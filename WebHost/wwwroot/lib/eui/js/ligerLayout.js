(function ($) {
    $.fn.ligerLayout = function (options) {
        return $.ligerui.run.call(this, "ligerLayout", arguments);
    };

    $.fn.ligerGetLayoutManager = function () {
        return $.ligerui.run.call(this, "ligerGetLayoutManager", arguments);
    };

    $.ligerDefaults.Layout = {
        topHeight: 50,
        bottomHeight: 50,
        leftWidth: 110,
        centerWidth: 300,
        rightWidth: 170,
        centerBottomHeight: 100,
        allowCenterBottomResize: true,
        border: true,
        inWindow: true,     //是否以窗口的高度为准 height设置为百分比时可用
        heightDiff: 0,     //高度补差
        height: '100%',      //高度
        onHeightChanged: null,
        isLeftCollapse: false,      //初始化时 左边是否隐藏
        isRightCollapse: false,     //初始化时 右边是否隐藏
        allowLeftCollapse: true,      //是否允许 左边可以隐藏
        allowRightCollapse: true,     //是否允许 右边可以隐藏
        allowLeftResize: true,      //是否允许 左边可以调整大小
        allowRightResize: true,     //是否允许 右边可以调整大小
        allowTopResize: true,      //是否允许 头部可以调整大小
        allowBottomResize: true,     //是否允许 底部可以调整大小
        space: 3, //间隔
        onEndResize: null,          //调整大小结束事件
        minLeftWidth: 80,            //调整左侧宽度时的最小允许宽度
        minRightWidth: 80,           //调整右侧宽度时的最小允许宽度
        onLeftToggle: null,  //左边收缩/展开事件
        onRightToggle: null  //右边收缩/展开事件
    };

    $.ligerMethos.Layout = $.ligerMethos.Layout || {};

})(jQuery);