(function ($) {
    $.fn.ligerMenuBar = function (options) {
        return $.ligerui.run.call(this, "ligerMenuBar", arguments);
    };
    $.fn.ligerGetMenuBarManager = function () {
        return $.ligerui.run.call(this, "ligerGetMenuBarManager", arguments);
    };

    $.ligerDefaults.MenuBar = {
        cls: "l-menubar-dark",
        items: null //{ id: 'add', text: '增加', click: itemclick, popup: "click", icon: "fa fa-car" },
    };

    $.ligerMethos.MenuBar = $.ligerMethos.MenuBar || {};

})(jQuery);