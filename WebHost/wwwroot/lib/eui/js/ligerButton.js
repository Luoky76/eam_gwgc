(function ($) {
    $.fn.ligerButton = function (options) {
        return $.ligerui.run.call(this, "ligerButton", arguments);
    };
    $.fn.ligerGetButtonManager = function () {
        return $.ligerui.run.call(this, "ligerGetButtonManager", arguments);
    };

    $.ligerDefaults.Button = {
        debounce: 300,//防抖
        width: null,
        type: "",
        text: '',
        disabled: false,
        onClick: null,
        icon: null
    };

    $.ligerMethos.Button = $.ligerMethos.Button || {};

})(jQuery);