(function ($) {
    $.fn.ligerTab = function (options) {
        return $.ligerui.run.call(this, "ligerTab", arguments);
    };

    $.fn.ligerGetTabManager = function () {
        return $.ligerui.run.call(this, "ligerGetTabManager", arguments);
    };

    $.ligerDefaults.Tab = {
        height: null,
        heightDiff: 0, // 高度补差
        inWindow: true,     //是否以窗口的高度为准 height设置为百分比时可用
        changeHeightOnResize: false,
        contextmenu: false,
        dblClickToClose: false, //是否双击时关闭
        dragToMove: false,    //是否允许拖动时改变tab项的位置
        showSwitch: false,       //显示切换窗口按钮
        showSwitchInTab: false, //切换窗口按钮显示在最后一项
        data: null, //传递数据容器
        onBeforeOverrideTabItem: null,
        onAfterOverrideTabItem: null,
        onBeforeRemoveTabItem: null,
        onAfterRemoveTabItem: null,
        onBeforeAddTabItem: null,
        onAfterAddTabItem: null,
        onBeforeSelectTabItem: null,
        onAfterSelectTabItem: null,
        onCloseOther: null,
        onCloseAll: null,
        onClose: null,
        onReload: null,
        pill: false,
        type: "",//brief,card
        onSwitchRender: null      //当切换窗口层构件时的事件
    };
    $.ligerDefaults.TabString = {
        closeMessage: "关闭当前页",
        closeOtherMessage: "关闭其他",
        closeAllMessage: "关闭所有",
        reloadMessage: "刷新"
    };

    $.ligerMethos.Tab = $.ligerMethos.Tab || {};

})(jQuery);