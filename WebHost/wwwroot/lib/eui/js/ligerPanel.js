(function ($) {
    $.fn.ligerPanel = function (options) {
        return $.ligerui.run.call(this, "ligerPanel", arguments);
    };

    $.ligerDefaults.Panel = {
        inWindow: true,
        heightDiff: 0,
        width: null,
        height: null,
        title: 'Panel',
        content: null,      //内容
        children: null, //其他控件
        url: null,          //远程内容Url
        urlParms: null,     //传参
        frameName: null,     //创建iframe时 作为iframe的name和id
        data: null,          //可用于传递到iframe的数据
        showClose: false,    //是否显示关闭按钮
        showToggle: true,    //是否显示收缩按钮
        showMax: true,    //是否显示最大化按钮
        showRefresh: false,    //是否显示刷新按钮
        type: "info",        //面板类型(用,分割) info,solid success,solid warning,solid error,solid
        onClose: null,       //关闭前事件
        onClosed: null,      //关闭事件
        onLoaded: null,           //url模式 加载完事件
        onToggle: null        //收缩/展开事件
    };

    $.ligerDefaults.PanelString = {
        refreshMessage: '刷新',
        closeMessage: '关闭',
        expandMessage: '展开',
        collapseMessage: '收起'
    };

    $.ligerMethos.Panel = $.ligerMethos.Panel || {};

})(jQuery);