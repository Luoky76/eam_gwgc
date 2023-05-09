(function ($) {
    $.ligerMenu = function (options) {
        return $.ligerui.run.call(null, "ligerMenu", arguments);
    };

    $.ligerDefaults.Menu = {
        wrap: null,
        width: null,
        top: 0,
        left: 0,
        cls: null,
        items: null //{ id: 'add', text: '增加', click: itemclick, dblclick: itemclick, icon: "fa fa-car" },
    };

    $.ligerMethos.Menu = $.ligerMethos.Menu || {};

    //旧写法保留
    $.ligerui.controls.Menu.prototype.setEnable = $.ligerui.controls.Menu.prototype.setEnabled;
    $.ligerui.controls.Menu.prototype.setDisable = $.ligerui.controls.Menu.prototype.setDisabled;
})(jQuery);