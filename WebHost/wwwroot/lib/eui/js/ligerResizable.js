(function ($) {
    $.fn.ligerResizable = function (options) {
        return $.ligerui.run.call(this, "ligerResizable", arguments,
            {
                idAttrName: 'ligeruiresizableid', hasElement: false, propertyToElemnt: 'target'
            });
    };

    $.fn.ligerGetResizableManager = function () {
        return $.ligerui.run.call(this, "ligerGetResizableManager", arguments,
            {
                idAttrName: 'ligeruiresizableid', hasElement: false, propertyToElemnt: 'target'
            });
    };

    $.ligerDefaults.Resizable = {
        handles: 'n, e, s, w, ne, se, sw, nw',
        handlebar: false,
        maxWidth: 2000,
        maxHeight: 2000,
        minWidth: 20,
        minHeight: 20,
        scope: 5,
        animate: false,
        onStartResize: function (e) { },
        onResize: function (e) { },
        onStopResize: function (e) { },
        onEndResize: null
    };

})(jQuery);