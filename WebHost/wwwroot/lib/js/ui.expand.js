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
        if (!window.session.Token) {
            return;
        }
        var g = this;
        var id = g._getCustomColumnId();
        var column = (topDomainWindow.__CustomColumn || {})[id];
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
        topDomainWindow.__CustomColumn = topDomainWindow.__CustomColumn || {};
        topDomainWindow.__CustomColumn[id] = column;
        g._setCustomColumn(column);
    },
    _setCustomColumn: function (columns) {
        if (!columns || !window.session.Token) {
            return;
        }
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
        topDomainWindow.__CustomColumn = topDomainWindow.__CustomColumn || {};
        topDomainWindow.__CustomColumn[id] = column;
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
$.ligerDialog.open = function (p, wid) {
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
        if (!window._dialogData) window._dialogData = new Object();
        window._dialogData[uid] = p;
        var win = window.open(url, '_blank', specs);
        win.id = wid || uid;
        $.ligerui.add(win);
        return win;
    }
    if (p.alone === true) return _alone(p, p.id);
    var dialog = $.ligerDialog._open(p);
    dialog.winalone = $('<i class="l-dialog-winbtn l-dialog-alone fa fa-desktop"></i>').prependTo(dialog.dialog.winbtns);//独立窗口
    dialog.winalone.click(function () {//独立弹出
        _alone(dialog.options);
        dialog.doClose();
    });
    return dialog;
};

//表单页面时长类型 支持 dd:hh:mm hh:mm mm 格式
liger.editors['duration'] = {
    control: 'TextBox',
    getValue: function (editor) {
        var val = editor.inputText.val();
        if (!val) return 0;
        var duration = val.split(/:|：/);
        duration = duration.map(Number);
        var minutes = 0, isNeg = false;
        if (duration.length > 0 && duration[0] < 0) {
            isNeg = true;
            duration[0] = - duration[0];
        }
        if (duration.length == 1) {
            minutes = duration[0];
        }
        else if (duration.length == 2) {
            minutes = duration[0] * 60 + duration[1];
        }
        else if (duration.length == 3) {
            minutes = duration[0] * 1440 + duration[1] * 60 + duration[2];
        }
        return isNeg ? parseInt(-minutes) : parseInt(minutes);
    },
    setValue: function (editor, value, editParm) {
        var text = "";
        if (value >= 0) {
            if (value >= 60) text += parseInt(value / 60) + ":";
            text += parseInt(value % 60);
        }
        else if (value < 0) {
            text += "-";
            value = -value;
            if (value >= 60) text += parseInt(value / 60) + ":";
            text += parseInt(value % 60);
        }
        editor.setValue(text, editParm.isTriggerEvent);
    }
};

//表格页面时长格式 支持 dd:hh:mm hh:mm mm 格式
$.ligerDefaults.Grid.formatters['duration'] = function (value, column) {
    var text = "";
    if (value >= 0) {
        if (value >= 60) text += parseInt(value / 60) + ":";
        text += parseInt(value % 60);
    }
    else if (value < 0) {
        text += "-";
        value = -value;
        if (value >= 60) text += parseInt(value / 60) + ":";
        text += parseInt(value % 60);
    }
    return text;
};

//表单附件的非涉密附件声明
$.extend($.ligerDefaults.Upload, {
    title: "<p>支持拖拽上传</p><p>格式：{extensions}</p >" //上传文字描述
        + "<p style='color:red; font-size:13px;'>严禁在本互联网非涉密平台处理、传输国家机密，请确认扫描、传输的文件资料不涉及国家机密。</p>" //非涉密附件声明
});