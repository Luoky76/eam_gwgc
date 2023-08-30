+(function ($) {

    //yyyy-MM-dd hh:mm:ss
    String.prototype.toDate = function (format) { };
    //yyyy-MM-dd hh:mm:ss
    Date.prototype.format || (Date.prototype.format = function (format) { });

    //几个默认的编辑器构造函数
    liger.editors = {
        "text": {
            control: 'TextBox'
        },
        "date": {
            control: 'DateEditor'
        },
        "combobox": {
            control: 'ComboBox',
            setValue: function (editor, value, editParm) {
                editor.setValue(value, editParm.isTriggerEvent);
            }
        },
        "spinner": {
            control: 'Spinner'
        },
        "checkbox": {
            control: 'CheckBox'
        },
        "checkboxlist": {
            control: 'CheckBoxList',
            body: $('<div></div>'),
            resize: function (editor, width) {
                editor.set('width', width);
            }
        },
        "radiolist": {
            control: 'RadioList',
            body: $('<div></div>'),
            resize: function (editor, width) {
                editor.set('width', width);
            }
        },
        "listbox": {
            control: 'ListBox',
            body: $('<div></div>'),
            resize: function (editor, width) {
                editor.set('width', width);
            }
        },
        "popup": {
            control: 'PopupEdit'
        },
        "number": {
            control: 'TextBox',
            options: { number: true },
            getValue: function (editor) {
                if (editor.getValue) {
                    var val = parseFloat(editor.getValue());
                    return isNaN(val) ? "" : val;
                }
            }
        },
        "ufloat": {
            control: 'TextBox',
            options: { number: true, clearPromptChar: true, promptChar: " ", mask: "ffffffffffffffffffff" }
        },
        "currency": {
            control: 'TextBox',
            options: { currency: true }
        },
        "digits": {
            control: 'TextBox',
            options: { digits: true },
            getValue: function (editor) {
                if (editor.getValue) {
                    var val = parseInt(editor.getValue(), 10);
                    return isNaN(val) ? "" : val;
                }
            }
        },
        "uint": {
            control: 'TextBox',
            options: { digits: true, clearPromptChar: true, promptChar: " ", mask: "000000000000000" }
        },
        "password": {
            control: 'TextBox'
        },
        "hidden": {
            control: 'Input'
        }
    };
    liger.editors["string"] = liger.editors["text"];
    liger.editors["select"] = liger.editors["combobox"];
    liger.editors["int"] = liger.editors["digits"];
    liger.editors["float"] = liger.editors["number"];
    liger.editors["chk"] = liger.editors["checkbox"];
    liger.editors["popupedit"] = liger.editors["popup"];
    liger.editors['dateStr'] = liger.editors["date"];
    liger.editors['dateFmt'];
    liger.editors['chkStr'] = liger.editors['checkboxString'];
    //扩展一个 多行文本框 的编辑器
    liger.editors['textarea'];
    //扩展一个 百分比输入框 的编辑器(0到1之间)
    liger.editors['percent'];

    //扩展一个 数字输入 的编辑器
    liger.editors['numberbox'];

    //ligerui 继承方法
    Function.prototype.ligerExtend = function (parent, overrides) {};
    //延时加载
    Function.prototype.ligerDefer = function (o, defer, args) {};
    // 核心对象
    window.liger = $.ligerui = {
        version: 'V2.0.0',
        managerCount: 0,
        //组件管理器池
        managers: {},
        managerIdPrev: 'ligerui',
        //管理器id已经存在时自动创建新的
        autoNewId: true,
        //错误提示
        error: {
            managerIsExist: "管理器id已经存在"
        },
        pluginPrev: 'liger',
        attrPrev: 'data',
        culture: {
            numberFormat: {
                pattern: ['-n'],
                decimals: 2,
                ',': ',',
                '.': '.',
                groupSize: [3],
                percent: {
                    pattern: [
                        '-n %',
                        'n %'
                    ],
                    decimals: 2,
                    ',': ',',
                    '.': '.',
                    groupSize: [3],
                    symbol: '%'
                },
                currency: {
                    name: '人民币',
                    abbr: '人民币',
                    pattern: [
                        '(n)',
                        '¥n'
                    ],
                    decimals: 2,
                    ',': ',',
                    '.': '.',
                    groupSize: [3],
                    symbol: '¥'
                }
            }
        },
        getId: function (prev) {
        },
        add: function (manager) {
        },
        remove: function (arg) {
        },
        //获取ligerui对象
        //1,传入ligerui ID
        //2,传入Dom Object
        get: function (arg, idAttrName) {
        },
        //根据类型查找某一个对象
        find: function (type) {
        },
        //扩展
        //1,默认参数
        //2,本地化扩展
        defaults: {},
        //3,方法接口扩展
        methods: {},
        //命名空间
        //核心控件,封装了一些常用方法
        core: {},
        //命名空间
        //组件的集合
        controls: {},
        //plugin 插件的集合
        plugins: {},
        _ns: ".liger",
        //事件命名空间
        NS: function (name) {
            return name + this._ns;
        },
        _activeElement: function (element) {
        },
        caret: function (element, start, end) {
        },
        debounce: debounce,//防抖
        throttle: throttle,//节流
        edgeBlur: function () {//移开焦点
        },
        topWindow: function () {
        }
    };

})(jQuery);