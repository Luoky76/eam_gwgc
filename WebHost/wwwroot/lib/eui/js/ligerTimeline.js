(function ($) {
    $.fn.ligerTimeline = function (options) {
        return $.ligerui.run.call(this, "ligerTimeline", arguments);
    };

    $.fn.ligerGetTimelineManager = function () {
        return $.ligerui.get(this);
    };

    $.ligerDefaults.Timeline = {
        data: null,//数据
        titleID: "title",//标题ID
        contentID: "content",//内容ID
        icon: null //图标重绘
    };
    $.ligerMethos.Timeline = $.ligerMethos.Timeline || {};

})(jQuery);