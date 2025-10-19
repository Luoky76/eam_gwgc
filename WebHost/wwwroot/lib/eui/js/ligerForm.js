(function ($) {
    $.fn.ligerForm = function (p) {
        p = p || {};
        if (p.EnterMoveNextControl !== undefined && p.enterMoveNextControl === undefined) p.enterMoveNextControl = p.EnterMoveNextControl;
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
        filter: null, //组合过滤条件，用于多字段查询(格式为FilterGroup或者FilterRlue数组) [{op: "equal",field: "xxx", type: "string"},{op: "equal",field: "ROLEDESC",type: "string"}]
        textField: null,       //文本框name
        type: null,             //表单类型
        editor: null,           //编辑器扩展
        label: null,            //Label
        labelInAfter: null,     //label显示在后面
        afterContent: null,     //后置内容
        beforeContent: null,    //前置内容
        initHide: null,             //默认是否隐藏，设置后会出现展开和收缩的按钮
        hideSpace: null,        //隐藏间隔
        hideLabel: null,        //隐藏label
        rightToken: null,       //label后面的分隔符
        attrRender: null,       //追加输入域的属性
        style: null,            //输入域的样式
        containerCls: null,     //输入域的样式类
        newline: null,          //换行显示
        op: null,               //操作符 参考ligerFilter
        vt: null,               //参数类型 int float date
        paramName: null,        //参数名 :paramName
        handleValue: null,      //值二次处理 (如果需要时间加1可以在此处理) handleValue(value)
        attr: null,             //属性列表 附加到input
        validate: null          //验证参数，比如required:true
    };

    $.ligerDefaults.Form_editor = {
    };
    $.ligerDefaults.Form_toConditions = function () {
        var g = this, p = g.options;
        var conditions = [];
        var groups = [];
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
                var defaultOp = (field.type === "text" || field.type === "string") ? "like" : "equal";
                var dbname = field.dbname || name.replace("_search_", "");
                var fieldOp = field.operator || field.op || defaultOp;
                var type = Object.prototype.toString.apply(value);
                var filter = field.filter;
                if (type === '[object Array]' && !filter) {
                    var isBetween = (fieldOp === "between");
                    var rules = [], ops = (isBetween ? ["greaterorequal", "lessorequal"] : ["equal"]);
                    for (var i = 0, l = value.length; i < l; i++) {
                        var item = value[i];
                        var itemOp = ops[(i > ops.length ? (ops.length - 1) : i)];
                        type = Object.prototype.toString.apply(item);
                        if (type === '[object Date]' && itemOp === "lessorequal") {
                            itemOp = "less";
                            item = $.ligerui.getDateEnd(item, editor.control.options.format);
                        }
                        rules.push({ op: itemOp, field: dbname, type: field.vt, value: item, paramName: (field.paramName ? (field.paramName + "_" + i) : field.paramName) });
                    }
                    filter = { op: (isBetween ? "and" : "or"), rules: rules };
                }
                if (filter) {
                    var rules = filter.rules ? filter.rules : filter;
                    var group = { op: filter.op || "or", rules: [] };
                    for (var i = 0, l = rules.length; i < l; i++) {
                        var item = rules[i];
                        group.rules.push({
                            op: item.op || item.operator || defaultOp,
                            field: item.field || field.dbname,
                            value: item.value || value,
                            type: item.type || item.vt,
                            paramName: item.paramName
                        });
                    }
                    if (group.rules.length > 0) groups.push(group);
                    return;
                }
                conditions.push({
                    op: fieldOp,
                    field: dbname, //加入Search的定义规则
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
        return {
            op: "and",
            rules: conditions,
            groups: groups
        };
    }
})(jQuery);