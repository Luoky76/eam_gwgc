/*
自定义列实现
*/
$.extend($.ligerui.controls.Grid.prototype, {
    _orginRendered: $.ligerui.controls.Grid.prototype._rendered,
    _rendered: function () {
        var g = this;
        g._orginRendered();
        g.bind("toggleCol", function (column, hide) {
            g._saveCustomColumn(column, hide);
        });
        g._loadCustomColumn();
    },
    _getCustomColumnId: function () {
        return location.pathname + "?" + getQueryStringByName("MenuNo") + "#" + this.id;
    },
    _loadCustomColumn: function () {
        var g = this;
        var id = g._getCustomColumnId();
        var column = (topWindow.__CustomColumn || {})[id];
        if (column !== undefined) {
            g._setCustomColumn(column);
            return;
        }
        $.ajax({
            noGlobal: true,
            async: false,
            type: 'post',
            dataType: 'json',
            url: 'myinfo/customColumn',
            data: { id: id },
            success: function (result) {
                column = result.Data;
            }
        });
        topWindow.__CustomColumn = topWindow.__CustomColumn || {};
        topWindow.__CustomColumn[id] = column;
        g._setCustomColumn(column);
    },
    _setCustomColumn: function (columns) {
        if (!columns) return;
        var g = this;
        var cols = (columns || "").split(',') || [];
        $(g.columns).each(function () {
            if (this.issystem) return;
            if (!this.name) return;
            if ($.inArray(this.name, cols) >= 0) {
                g._setColumnVisible(this, true);
            }
        });
    },
    _saveCustomColumn: function (e) {
        var g = this, p = this.options;
        if (!p.allowHideColumn) return;
        var columns = [];
        $(g.columns).each(function () {
            if (this.issystem) return;
            if (!this.name) return;
            if (!this._hide) return;
            columns.push(this.name);//只记录隐藏列
        });
        var column = columns.join(',');
        var id = g._getCustomColumnId();
        topWindow.__CustomColumn = topWindow.__CustomColumn || {};
        topWindow.__CustomColumn[id] = column;
        $.ajax({
            noGlobal: true,
            async: true,
            type: 'post',
            dataType: 'json',
            url: 'myinfo/customColumnSave',
            data: { id: id, columns: column }
        });
    }
});

/*
独立弹出
*/
$.ligerDialog._open = $.ligerDialog.open;
$.ligerDialog.open = function (p) {
    var isDialog = p.modal || !p.url || p.type || p.onClose || p.isDialog;
    if (!isDialog) isDialog = (window.localStorage.getItem("dialogType") || "1") !== "1";
    if (isDialog || p.title === null) return $.ligerDialog._open(p);
    var _alone = function (p) {
        p.onUnload = function (child) {
            setTimeout(function () {
                if (!child || child.closed) {
                    if (p.onClosed) p.onClosed();
                    if (window._dialogData && p.uid) delete window._dialogData[uid];
                    if (child.id) $.ligerui.remove(child.id);
                }
            }, 200);
        }
        var uid = (new Date()).format('ddHHmmssfff');
        p.uid = uid;
        var url = p.url || "";
        var specs = "menubar=no,toolbar=no,location=no,personalbar=no,status=no,resizable=no,titlebar=no";
        if (p.top) specs += ",top:" + p.top;
        if (p.left) specs += ",left:" + p.left;
        //if (p.width) specs += ",width:" + p.width;
        //if (p.height) specs += ",height:" + p.width;
        if (p.isResize) specs += ",resizable";
        if (url.toLowerCase().indexOf("http") !== 0 && url.indexOf("/") !== 0) {
            var curWindow = window;
            if (location.pathname.indexOf("dialog.html") > -1 && window.opener) curWindow = window.opener;
            url = curWindow.location.protocol + "//" + curWindow.location.host + curWindow.location.pathname.substring(0, curWindow.location.pathname.lastIndexOf("/")) + "/" + url;
        }
        url = gksybConfigs.urlBase + "dialog.html?uid=" + uid + "&url=" + encodeURIComponent(url) + "&title=" + encodeURIComponent(p.title);
        if (!window.dialogData) window._dialogData = new Object();
        window._dialogData[uid] = p;
        return window.open(url, '_blank', specs);
    }
    var dialog = $.ligerDialog._open(p);
    dialog.winalone = $('<i class="l-dialog-winbtn l-dialog-alone fa fa-desktop"></i>').prependTo(dialog.dialog.winbtns);//独立窗口
    dialog.winalone.click(function () {//独立弹出
        var win = _alone(dialog.options);
        win.id = dialog.id;
        dialog.doClose();
        $.ligerui.add(win);
    });
    return dialog;
};