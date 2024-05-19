; (function ($) {
    'use strict';
    liger.editors['Ueditor'] = {
        create: function (container, editParm, p) {
            var inputText = $('<div type="text/plain"></div>');
            var id = (p.prefixID || "") + editParm.field.name;
            inputText.attr({ id: id, name: id });
            container.append(inputText);
            p = $.extend({}, p);
            p = editParm.field ? $.extend({}, editParm.field.options) : $.extend({}, editParm.column.editor, editParm.column.editor.options);
            p.height = p.height || (editParm.field ? editParm.field.height : editor.column.height);
            var editor = UE.getEditor(id, p);
            editor.ready(function () {
                if (p.height) {
                    editor.setHeight(p.height - ($(editor.ui.getDom("toolbarbox")).height() || 0));
                }
                if (p.disabled) {
                    editor.setDisabled();
                }
            });
            $.extend(editor, {//设置只读
                inputText: inputText,
                _setReadonly: function (value) {
                    value ? this.setDisabled() : this.setEnabled();
                },
                _setDisabled: function (value) {
                    value ? this.setDisabled() : this.setEnabled();
                },
                _getValue: function () {
                    return encrypt(this.getContent());
                },
                _setValue: function (value, isTriggerEvent) {
                    this.ready(function () {
                        this.setContent(value);
                    });
                }
            });
            editor.setReadonly = editor._setReadonly;
            editor.getValue = editor._getValue;
            editor.setValue = editor._setValue;
            editor.setText = editor._setValue;
            return editor;
        }
    };

    UE.ajax.orginRequest = UE.ajax.request;
    UE.ajax.request = function (url, options) {
        if (typeof url === "object") {
            options = url;
            url = options.url;
        }
        if (!url) return;
        options.url = url;
        options.successInner = options.onsuccess;
        options.errorInner = options.onerror;
        delete options.onsuccess;
        delete options.onerror;
        var p = $.extend(true, {
            async: true,
            jsToken: true,
            dataType: 'json',
            type: 'post',
            cache: false,
            success: function (result, statusText, jqXHR) {
                if (options.successInner) options.successInner(jqXHR);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                if (options.errorInner) options.errorInner();
            }
        }, options);
        $.ajax(p);
    };

    /**
    * 自定义上传接口
    * 由于所有Neditor请求都通过editor对象的getActionUrl方法获取上传接口，可以直接通过复写这个方法实现自定义上传接口
    * @param {String} action 匹配neditor.config.js中配置的xxxActionName
    * @returns 返回自定义的上传接口
    */
    UE.Editor.prototype._bkGetActionUrl = UE.Editor.prototype.getActionUrl;
    UE.Editor.prototype.getActionUrl = function (action) {
        /* 按config中的xxxActionName返回对应的接口地址 */
        var url = this._bkGetActionUrl.call(this, action);
        return gksybConfigs.getUrl(url);
    }
})(jQuery);