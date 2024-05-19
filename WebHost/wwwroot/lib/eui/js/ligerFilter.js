(function ($) {
    $.fn.ligerFilter = function () {
        return $.ligerui.run.call(this, "ligerFilter", arguments);
    };

    $.fn.ligerGetFilterManager = function () {
        return $.ligerui.run.call(this, "ligerGetFilterManager", arguments);
    };

    $.ligerDefaults.Filter = {
        //字段列表
        fields: [],
        //字段类型 - 运算符 的对应关系
        operators: {},
        //自定义输入框(如下拉框、日期)
        editors: {},
        buttonCls: null
    };
    $.ligerDefaults.FilterString = {
        strings: {
            "and": "并且",
            "or": "或者",
            "equal": "等于",
            "notequal": "不等于",
            "startwith": "以..开始",
            "endwith": "以..结束",
            "like": "包含",
            "notlike": "不包含",
            "greater": "大于",
            "greaterorequal": "大于或等于",
            "less": "小于",
            "lessorequal": "小于或等于",
            "in": "包括在...",
            "notin": "不包括...",
            "isnull": "为空",
            "isnotnull": "不为空",
            "addgroup": "添加分组",
            "addrule": "添加条件",
            "deletegroup": "删除分组"
        }
    };

    $.ligerDefaults.Filter.operators['string'] =
        $.ligerDefaults.Filter.operators['text'] =
        ["equal", "notequal", "startwith", "endwith", "like", "notlike", "greater", "greaterorequal", "less", "lessorequal", "in", "notin", "isnull", "isnotnull"];

    $.ligerDefaults.Filter.operators['number'] =
        $.ligerDefaults.Filter.operators['int'] =
        $.ligerDefaults.Filter.operators['float'] =
        $.ligerDefaults.Filter.operators['date'] =
        ["equal", "notequal", "greater", "greaterorequal", "less", "lessorequal", "in", "notin", "isnull", "isnotnull"];

})(jQuery);