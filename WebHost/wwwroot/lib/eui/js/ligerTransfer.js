(function ($) {
    $.fn.ligerTransfer = function (options) {
        return $.ligerui.run.call(this, "ligerTransfer", arguments);
    };

    $.fn.ligerGetTransferManager = function () {
        return $.ligerui.get(this);
    };

    $.ligerDefaults.Transfer = {
        title: "请选择",
        width: null,
        height: null,
        url: null,
        data: null,
        valueField: 'ID',       //值成员
        textField: 'TEXT',      //显示成员
        value: null,
        split: ",",
        columns: null,
        render: null
    };
    $.ligerMethos.Transfer = $.ligerMethos.Transfer || {};

})(jQuery);