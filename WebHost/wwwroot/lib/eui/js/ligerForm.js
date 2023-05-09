(function ($) {
    $.fn.ligerForm = function () {
        return $.ligerui.run.call(this, "ligerForm", arguments);
    };

    $.ligerui.getConditions = function (form, options) {
        if (!form) return null;
        form = liger.get($(form));
        if (form && form.toConditions) return form.toConditions();
    };

    $.ligerDefaults = $.ligerDefaults || {};
    $.ligerDefaults.Form = {
        width: null,    // 表单的宽度
        //控件宽度
        inputWidth: 180,
        //标签宽度
        labelWidth: "auto",
        //间隔宽度
        space: 15,
        rightToken: '：',
        //标签对齐方式
        labelAlign: 'right',
        //控件对齐方式
        align: 'left',

        autoTypePrev: 'ui-',
        //字段
        /*
        数组的集合,支持的类型包括在$.ligerDefaults.Form.editors,这个editors同Grid的editors继承于base.js中提供的编辑器集合,具体可以看liger.editors
        字段的参数参考 127行左右的 $.ligerDefaults.Form_fields,
        ui内置的编辑表单元素都会调用ui的表单插件集合,所以这些字段都有属于自己的"liger对象",可以同liger.get("[ID]")的方式获取，这里的[ID]获取方式优先级如下：
        1,定义了field.id 则取field.id
        2,如果是下拉框和PopupEdit，并且定义了comboboxName，则取comboboxName(如果表单定义了prefixID,需要加上)
        3,默认取field.name(如果表单定义了prefixID,需要加上)
        */
        fields: [],
        //创建的表单元素是否附加ID
        appendID: true,
        //生成表单元素ID、Name的前缀
        prefixID: null,
        //json解析函数
        toJSON: $.ligerui.toJSON,
        labelCss: null,
        fieldCss: null,
        spaceCss: null,
        onAfterSetFields: null,//如果有tab 请用onRendered替代
        // 参数同 ligerButton
        buttons: null,              //按钮组
        //readonly: false,              //是否只读 2014年5月28日去除防止元素设置无作用
        editors: {},              //编辑器集合,使用同$.ligerDefaults.Grid.editors
        //验证
        validate: null,
        tab: null,
        clsTab: 'ui-tabs-nav ui-helper-clearfix',
        clsTabItem: 'ui-state-default',
        clsTabItemSelected: 'ui-tabs-selected',
        clsTabContent: 'ui-tabs-panel ui-widget-content',
        enterMoveNextControl: true
    };

    $.ligerDefaults.FormString = {
        invalidMessage: '存在{errorCount}个字段验证不通过，请检查!',
        detailMessage: '详细',
        okMessage: '确定'
    };

    $.ligerDefaults.Form_fields = {
        name: null,             //字段name
        dbname: null,          //数据库实际名称
        userSearch: null,        //是否用于组成查询条件配合grid
        textField: null,       //文本框name
        type: null,             //表单类型
        editor: null,           //编辑器扩展
        label: null,            //Label
        labelInAfter: null,  //label显示在后面
        afterContent: null,  //后置内容
        beforeContent: null, //前置内容
        hideSpace: null,
        hideLabel: null,
        rightToken: null,
        attrRender: null,
        style: null,
        containerCls: null,
        newline: null,          //换行显示
        op: null,               //操作符 附加到input
        vt: null,               //值类型 附加到input
        attr: null,             //属性列表 附加到input
        validate: null          //验证参数，比如required:true
    };

    $.ligerDefaults.Form_editor = {
    };
    $.ligerDefaults.Form_toConditions = function () {
        var g = this, p = g.options;
        var conditions = [];
        $(p.fields).each(function (fieldIndex, field) {
            if (field.userSearch === false) return;
            var name = field.name, editor = g.editors[fieldIndex];
            if (!editor || !name) return;
            var value = editor.editor.getValue.call(g, editor.control, {
                field: field
            });
            if (!(typeof value === "number" && isNaN(value)) && value != null && value !== "") {
                if (field.prefixValue) value = field.prefixValue + value;
                if (field.suffixValue) value = value + field.suffixValue;
                if (field.handleValue) value = field.handleValue(value);
                conditions.push({
                    op: field.operator || field.op || ((field.type === "text" || field.type === "string") ? "like" : "equal"),
                    field: field.dbname || name.replace("_search_", ""), //加入Search的定义规则
                    value: value,
                    type: field.vt,
                    paramName: field.paramName
                });
            }
            else if (field.paramName) {
                conditions.push({
                    op: "NULLPARAM",
                    field: "", //加入Search的定义规则
                    value: null,
                    type: field.vt,
                    paramName: field.paramName
                });
            }
        });
        return conditions;
    }
})(jQuery);