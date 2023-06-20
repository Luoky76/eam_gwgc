(function ($) {
    $.fn.ligerVirtualDom = function (options) {
        return $.ligerui.run.call(this, "ligerVirtualDom", arguments);
    };

    $.fn.ligerGetVirtualDomManager = function () {
        return $.ligerui.run.call(this, "ligerGetVirtualDomManager", arguments);
    };

    $.ligerDefaults.VirtualDom = {
        parentEl: null,//虚拟滚动的父节点，默认取parent
        skipDomHeight: false,//跳过设置节点高度，用于table
        bufferSize: 1,//缓存范围
        cache: false,//是否开启缓存
        data: null,//数据
        itemHeight: 32,//节点高度,可以是函数
        heightDiff: 0,//高度补差
        itemCls: "virtual",
        renderItem: function (item, index, top, fromTop) {//渲染节点 可用相对定位、绝对定位、前后加空白三种方式实现，table由于ie定位限制，只能用前后加空白实现
            var g = this;
            var position = index > parseInt(g.data.length / 2) ? "bottom:" + (g.domHeight - top - g.height[index]) : "top:" + top;
            return '<div class="virtual" style="position:absolute;' + position + 'px;">' + item.toString() + '</div>';
        },
        renderExternal: null,//渲染追加
        renderCustomScreen: null,//自定义绘制区域
        onAfterRenderScreen: null
    };

    $.ligerMethos.VirtualDom = $.ligerMethos.VirtualDom || {};

    $.ligerui.controls.VirtualDom.prototype.setData = $.ligerui.controls.VirtualDom.prototype._setData;
})(jQuery);