//window.UEDITOR_HOME_URL = window.gksybConfigs.urlBase + "/lib/neditor/"
document.write('<script src="' + bootPATH + 'neditor/neditor.config.js?ver=' + JsVersion + '"  type="text/javascript" charset="utf-8" ></sc' + 'ript>');
if (url.indexOf("/localhost") >= 0) {
    document.write('<script src="' + bootPATH + 'neditor/neditor.js?ver=' + JsVersion + '" type="text/javascript" charset="utf-8" ></sc' + 'ript>');
}
else {
    document.write('<script src="' + bootPATH + 'neditor/neditor.min.js?ver=' + JsVersion + '" type="text/javascript" charset="utf-8" ></sc' + 'ript>');
}
document.write('<script src="' + bootPATH + 'neditor/i18n/zh-cn/zh-cn.js?ver=' + JsVersion + '" type="text/javascript"  charset="utf-8" ></sc' + 'ript>');
document.write('<script src="' + bootPATH + 'neditor/neditor.service.js?ver=' + JsVersion + '"  type="text/javascript" charset="utf-8" ></sc' + 'ript>');

$.ligerDefaults.Form.editors.Ueditor =
{
    create: function (container, editParm, p) {
        var inputBody = $('<div type="text/plain"></div>');
        var id = (p.prefixID || "") + editParm.field.name;
        if ($("#" + id).length) {
            editor = $("#" + id);
        }
        inputBody.attr({
            id: id,
            name: id
        });
        container.append(inputBody);
        var uEditorOptions = {};
        uEditorOptions = $.extend(uEditorOptions, editParm.field.options);
        editor = UE.getEditor(id, uEditorOptions);
        if (editor.isReady) {
            if (editParm.field.height) {
                editor.setHeight(editParm.field.height - ($(editor.ui.getDom("toolbarbox")).height() || 0))
            }
        }
        else {
            editor.ready(function () {
                if (editParm.field.height) {
                    editor.setHeight(editParm.field.height - ($(editor.ui.getDom("toolbarbox")).height() || 0))
                }
            });
        }
        $.extend(editor, {//设置只读
            _setReadonly: function (readonly) {
                if (this.isReady) this.setDisabled();
                else {
                    this.options.readonly = true;
                }
            },
            //设置不可用
            _setDisabled: function (value) {
                if (this.isReady) this.setDisabled();
                else {
                    this.options.readonly = true;
                }
            },
            setReadonly: function (readonly) {
                return this._setReadonly(readonly);
            }
        });
        return editor;
    },
    getValue: function (editor, editParm) {
        if (!editor.isReady) return null;
        var value = editor.getContent();
        value = encrypt(value);
        return value;
    },
    setValue: function (editor, value, editParm) {
        if (editor.isReady) editor.setContent(value);
        else {
            editor.ready(function () {
                this.setContent(value);
            });
        }
    },
    getText: function (editor, editParm) {
        if (!editor.isReady) return null;
        var value = editor.getContentTxt();
        value = encrypt(value);;
        return value;
    },
    setText: function (editor, value, editParm) {
        if (editor.isReady) editor.setContent(value);
        else {
            editor.ready(function () {
                this.setContent(value);
            });
        }
    },
    resize: function (editor, width, height, editParm) {
    },
    setEnabled: function (editor, isEnabled) {
        if (isEnabled) {
            if (editor.setEnabled) editor.setEnabled();
        }
        else {
            if (editor.setDisabled) editor.setDisabled();
        }
    }
};