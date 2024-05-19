(function ($) {
    $.fn.ligerToolBar = function (options) {
        return $.ligerui.run.call(this, "ligerToolBar", arguments);
    };

    $.fn.ligerGetToolBarManager = function () {
        return $.ligerui.run.call(this, "ligerGetToolBarManager", arguments);
    };

    $.ligerDefaults.ToolBar = {
        type: "info",
        items: null //{ id: 'add', text: '增加', click: itemclick, popup: "click", icon: "fa fa-car" },
    };

    $.ligerMethos.ToolBar = $.ligerMethos.ToolBar || {};

    //旧写法保留
    $.ligerui.controls.ToolBar.prototype.setEnable = $.ligerui.controls.ToolBar.prototype.setEnabled;
    $.ligerui.controls.ToolBar.prototype.setDisable = $.ligerui.controls.ToolBar.prototype.setDisabled;
})(jQuery);