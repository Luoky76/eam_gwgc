(function ($) {
    $.fn.ligerAccordion = function (options) {
        return $.ligerui.run.call(this, "ligerAccordion", arguments);
    };

    $.fn.ligerGetAccordionManager = function () {
        return $.ligerui.get(this);
    };

    $.ligerDefaults.Accordion = {
        height: null,
        accordion: true,//手风琴模式
        speed: "normal",
        changeHeightOnResize: false,
        heightDiff: 0 // 高度补差
    };
    $.ligerMethos.Accordion = $.ligerMethos.Accordion || {};

})(jQuery);