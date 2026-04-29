(function ($) {
    var l = $.ligerui;

    $.fn.ligerGrid = function (p) {
        p = p || {};
        //命名纠正
        if (p.AutoWidth !== undefined && p.autoWidth === undefined) p.autoWidth = p.AutoWidth;
        if (p.AppendSelectColumns !== undefined && p.appendSelectColumns === undefined) p.appendSelectColumns = p.AppendSelectColumns;
        if (p.AppendGroupByColumns !== undefined && p.appendGroupByColumns === undefined) p.appendGroupByColumns = p.AppendGroupByColumns;
        if (p.DataPrivilege !== undefined && p.dataPrivilege === undefined) p.dataPrivilege = p.DataPrivilege;
        if (p.EnterMoveNextControl !== undefined && p.enterMoveNextControl === undefined) p.enterMoveNextControl = p.EnterMoveNextControl;
        //不分页本地处理数据
        if (p.crosstab) p.usePager = false;
        var usePager = (p.usePager !== undefined) ? p.usePager : $.ligerDefaults.Grid.usePager;
        if (usePager === false) {
            p.dataAction = "local";
        }
        var size = $.ligerui.getScrollbarSize();
        if (p.scrollWidth === undefined) p.scrollWidth = size.width;
        if (p.scrollHeight === undefined) p.scrollHeight = size.height;
        if (p.isMultiSelect && p.allowUnSelectRow === undefined) p.allowUnSelectRow = true;
        return $.ligerui.run.call(this, "ligerGrid", arguments);
    };

    $.fn.ligerGetGridManager = function () {
        return $.ligerui.run.call(this, "ligerGetGridManager", arguments);
    };

    $.ligerDefaults.Grid = {
        title: null,
        width: 'auto', //宽度值
        minHeight: 150, //最小高度
        height: 'auto', //高度值
        columnWidth: null, //默认列宽度
        resizable: true, //table是否可伸缩
        url: false, //ajax url
        urlParms: null, //url带参数
        data: null, //初始化数据
        usePager: true, //是否分页
        hideLoadButton: false, //是否隐藏刷新按钮
        pagerRender: null, //分页栏自定义渲染函数
        page: 1, //默认当前页
        pageSize: 20, //每页默认的结果数
        pageSizeOptions: [20, 40, 60, 100, 500, 1000, 2000, 5000, 9999], //可选择设定的每页结果数
        parms: [], //提交到服务器的参数
        columns: [], //数据源
        minColToggle: 1, //最小显示的列
        dataType: 'server', //数据源：本地(local)或(server),本地是将读取p.data。不需要配置，取决于设置了data或是url
        dataAction: 'server', //提交数据的方式：本地(local)或(server),选择本地方式时将在客服端分页、排序。
        showTableToggleBtn: false, //是否显示'显示隐藏Grid'按钮
        switchPageSizeApplyComboBox: false, //切换每页记录数是否应用ligerComboBox
        allowAdjustColWidth: true, //是否允许调整列宽
        checkbox: true, //是否显示复选框
        frozenCheckbox: true, //复选框按钮是否在固定列中
        checkboxColWidth: 40, //复选框列宽度
        checkboxIndex: 2, //复选框列位置
        isSingleCheck: false, //复选框选择的时候是否单选模式
        isMultiSelect: null, //点击行进行多选 默认只有点击checkbox多选
        allowHideColumn: true, //是否显示'切换列层'按钮
        enabledEdit: false, //是否允许编辑
        isScroll: true, //是否滚动
        scrollWidth: 20,//滚动条宽度，用于计算宽度
        scrollHeight: 16,//滚动条高度，用于计算锁定高度
        dateFormat: 'yyyy-MM-dd', //默认时间显示格式
        inWindow: true, //是否以窗口的高度为准 height设置为百分比时可用，为对象时根据对象的高度处理
        statusName: '__status', //状态名
        method: 'post', //获取数据http方式
        async: true,
        fixedCellHeight: false, //是否固定单元格的高度
        heightDiff: 0, //高度补差,当设置height:100%时，可能会有高度的误差，可以通过这个属性调整
        css: null, //类名
        root: 'Rows', //数据源字段名
        record: 'Total', //数据源记录数字段名
        pageParmName: 'page', //页索引参数名，(提交给服务器)
        pagesizeParmName: 'pagesize', //页记录数参数名，(提交给服务器)
        sortnameParmName: 'sortname', //页排序列名(提交给服务器)
        sortorderParmName: 'sortorder', //页排序方向(提交给服务器)
        initSortDisplay: false,//是否展示初始排序
        holdSortName: null,//固定排序
        allowUnSelectRow: false, //是否允许反选行
        alternatingRow: true, //奇偶行效果
        mouseoverRowCssClass: 'l-grid-row-over',
        enabledSort: true, //是否允许排序
        rowClsRender: null, //行自定义css class渲染器
        rowAttrRender: null, //行自定义属性渲染器(包括style，也可以定义)
        rowRender: null, //自定义行html（空返回则不重新渲染）
        groupColumnName: null, //分组 - 列名
        groupColumnDisplay: '', //分组 - 列显示名字
        groupRowHeight: 43,//分组行高度
        groupTotalHeight: 42,//分组统计行高度
        groupInitHide: false,//分组初始是否隐藏
        groupRender: null, //分组 - 渲染器
        treeGroupColumnName: null,//树形分组
        treeGroupInitExtend: null,//初始展开状态
        treeGroupRender: null,//树形分组 - 渲染器
        totalRender: null, //统计行(全部数据)
        delayLoad: false, //初始化时是否不加载
        where: null, //数据过滤查询函数,(参数一 data item，参数二 data item index)
        selectRowButtonOnly: false, //复选框模式时，是否只允许点击复选框才能选择行
        selectable: true, //是否可选择
        whenRClickToSelect: false, //右击行时是否选中
        ajaxOptions: null,//ajax扩展属性
        contentType: null, //Ajax contentType参数
        clickToEdit: true, //是否点击单元格的时候就编辑
        detailToEdit: false, //是否点击明细的时候进入编辑
        onEndEdit: null, //结束编辑事件
        minColumnWidth: null,//列最小宽度
        tree: null, //treeGrid模式
        crosstab: false,//交叉表模式
        rowKey: null,//行主键，指定后保留上次选择
        isChecked: null, //复选框 初始化函数
        isSelected: null, //选择 初始化函数
        frozen: false, //是否固定列
        detail: null, //明细列
        frozenDetail: false, //明细按钮是否在固定列中
        detailColWidth: 40, //明细列宽度
        detailIndex: 3, //明细列位置
        detailHeight: 260, //明细列高度
        isShowDetailToggle: null, //是否显示展开/收缩明细的判断函数
        rownumbers: false, //是否显示行序号
        frozenRownumbers: true, //行序号是否在固定列中
        rownumbersColWidth: 40, //序号列宽度
        rownumbersName: "#", //序号列名称
        rownumbersIndex: 1,//序号列位置
        colDraggable: true, //是否允许表头拖拽
        rowDraggable: false, //是否允许行拖拽 {true:允许序号列和选择框列拖拽,"row":允许整行拖拽}
        rowDraggingRender: null, //行拖动时渲染函数
        autoCheckChildren: true, //是否自动选中子节点
        rowHeight: 32, //行默认的高度
        headerRowHeight: 32, //表头行的高度
        toolbar: null, //工具条,参数同 ligerToolbar的,额外参数有title、icon
        toolbarShowInLeft: false, //工具条显示在左边
        headerImg: null, //表格头部图标
        autoFilter: false, //自动生成高级查询, 需要filter/toolbar组件支持. 需要引用skins/ligerui-icons.css
        rowSelectable: true, //是否允许选择
        scrollToPage: false, //滚动时分页
        onDragCol: null, //拖动列事件
        onToggleCol: null, //切换列事件
        onChangeSort: null, //改变排序事件
        onSuccess: null, //成功获取服务器数据的事件
        onDblClickRow: null, //双击行事件
        onSelectRow: null, //选择行事件
        onBeforeSelectRow: null, //选择前事件
        onUnSelectRow: null, //取消选择行事件
        onBeforeCheckRow: null, //选择前事件，可以通过return false阻止操作(复选框)
        onCheckRow: null, //选择事件(复选框)
        onBeforeCheckAllRow: null, //选择前事件，可以通过return false阻止操作(复选框 全选/全不选)
        onCheckAllRow: null, //选择事件(复选框 全选/全不选)onextend
        onBeforeShowData: null, //显示数据前事件，可以通过reutrn false阻止操作
        onAfterShowData: null, //显示完数据事件
        onError: function (XMLHttpRequest, _textStatus, errorThrown) { //错误事件
            $.ligerDialog.error('请求数据出错,页面即将跳转!<br/>原因为：' + (XMLHttpRequest.responseText || "") + "<br/>错误码:" + (XMLHttpRequest.status || "") + (errorThrown || ""), "操作失败", function () {
                location.reload();
            });
        },
        onSubmit: null, //提交前事件
        onReload: null, //刷新事件，可以通过return false来阻止操作
        onToFirst: null, //第一页，可以通过return false来阻止操作
        onToPrev: null, //上一页，可以通过return false来阻止操作
        onToNext: null, //下一页，可以通过return false来阻止操作
        onToLast: null, //最后一页，可以通过return false来阻止操作
        onAfterAddRow: null, //增加行后事件
        onBeforeEdit: null, //编辑前事件
        onBeforeSubmitEdit: null, //验证编辑器结果是否通过
        onAfterEdit: null, //结束编辑后事件
        onLoading: null, //加载时函数
        onLoaded: null, //加载完函数
        onContextmenu: null, //右击事件
        onBeforeCancelEdit: null, //取消编辑前事件
        onAfterSubmitEdit: null, //提交后事件
        onRowDragDrop: null, //行拖拽后事件
        onGroupExtend: null, //分组展开事件
        onGroupCollapse: null, //分组收缩事件
        onTreeExpand: null, //树展开事件
        onTreeCollapse: null, //树收缩事件
        onTreeExpanded: null, //树展开事件
        onTreeCollapsed: null, //树收缩事件
        onLoadData: null, //加载数据前事件
        onHeaderCellBuild: null, //标题列创建事件
        onHeaderMenuBuild: null,//表头菜单创建时，拦截用于加入自定义功能
        onlySelectColumns: false, //是否只查询grid.columns的内容
        appendSelectColumns: null, //onlySelectColumns为true时起作用,追加查询列
        appendGroupByColumns: null, //追加group by 列
        dataPrivilege: true, //启用数据规则
        autoWidth: false, //自动列宽
        sortFix: false, //排序后缀
        useVirtualDom: true, //使用虚拟dom
        isFilter: true, //允许筛选
        enterMoveNextControl: true //回车变tab
    };
    $.ligerDefaults.GridString = {
        errorMessage: '发生错误',
        selectMessage: '选中：<b>{select}</b>，',
        pageStatMessage: '当前：<b>{totalCurrent}</b>，总：<b>{total}</b>',//显示从{from}到{to}，总 {total} 条 。每页显示：{pagesize}
        shortPageStatMessage: '总：<b>{total}</b>',
        pageTextMessage: 'Page',
        loadingMessage: '',
        findTextMessage: '查找',
        noRecordMessage: '没有符合条件的记录存在',
        isContinueByDataChanged: '数据已经改变,如果继续将丢失数据,是否继续?',
        cancelMessage: '取消',
        saveMessage: '保存',
        applyMessage: '应用',
        draggingMessage: '{count}行'
    };

    $.ligerDefaults.Grid_columns = {
        id: null, //自定义id 默认为c10列位置
        name: null, //名称
        dbname: null, //数据库实际名称
        sortType: null, //排序类型 string float
        sortdbname: null, //排序用数据库实际名称
        sortFix: false, //排序后缀
        totalSummary: null,//{name:"默认为当前列，可指定其他列进行统计",isDisplay:true,type:"sum,tsum,count,max,min,avg",hastext:false,igronNull:false,render:function(info, column, allData, groupData){}}
        display: null, //显示名称
        headerRender: null, //标题头渲染函数 function(column)
        isAllowHide: true, //允许隐藏
        isSort: false, //允许排序
        isFilter: true, //允许筛选
        type: null, //数据类型 string(text),date,int,float(number) 括号内是别名
        columns: null,//多级表头[{name:''}]
        frozen: false, //浮动 true false right
        width: 120, //初始宽度
        minWidth: 80, //最小宽度
        maxWidth: null, //最大宽度
        appendWidth: null,//追加宽度 用于自动宽度算不准的情况
        format: null, //格式化 'yyyy-MM-dd hh:mm:ss'或者针对select{data:null,formatRender:function(texts, rowdata, column, ids),precision:'小数位数配合numberbox'}
        formatType: null, //格式化类型不指定则取type  select,date,chk,numberbox,currency
        headerAlign: null, //标题的text-align属性
        align: 'center', //内容的text-align属性
        cls: null, //列样式 给标题列和具体的内容列追加样式，这样可以进行样式重载
        hide: false, //默认是否隐藏
        editor: null, //行内编辑器 {type: 'text',options:{  onChangeValue: function (input, value, g) { }}} options同表单元素一致
        render: null, //单元格渲染器 function(rowdata, rowindex, value, column)
        mergeColumn: null, //合并单元格 true或者具体的列名（可指定根据其他列合并）
        crosstab: false,//交叉列 true null false 设定后会根据值生成列 设置false则跳过列处理（不分组，取第一行数据）
        values: null,//交叉列对应的统计值,列名或者函数function (rows, name)
        textField: null //真正显示的字段名,如果设置了，在编辑状态时,会调用创建编辑器的setText和getText方法
    };
    $.ligerDefaults.Grid_editor = {
        type: null,
        ext: null,
        onChange: null,
        onChanged: null
    };
    //接口方法扩展
    $.ligerMethos.Grid = $.ligerMethos.Grid || {};

    //排序器扩展
    $.ligerDefaults.Grid.sorters = $.ligerDefaults.Grid.sorters || {};

    //格式化器扩展
    $.ligerDefaults.Grid.formatters = $.ligerDefaults.Grid.formatters || {};

    //编辑器扩展
    $.ligerDefaults.Grid.editors = $.ligerDefaults.Grid.editors || {};

    $.ligerDefaults.Grid.sorters['date'];
    $.ligerDefaults.Grid.sorters['int'];
    $.ligerDefaults.Grid.sorters['float'] = $.ligerDefaults.Grid.sorters['number'];
    $.ligerDefaults.Grid.sorters['ascii'] = function (val1, val2) {
        if (typeof val1 !== "string") val1 = (val1 || "").toString();
        if (typeof val2 !== "string") val2 = (val2 || "").toString();
        var minLength = Math.min(val1.length, val2.length);
        for (var i = 0; i < minLength; i++) {
            var codeA = val1.charCodeAt(i);
            var codeB = val2.charCodeAt(i);
            if (codeA !== codeB) {
                return codeA < codeB ? -1 : 1;
            }
        }
        if (val1.length === val2.length) {
            return 0;
        }
        return val1.length < val2.length ? -1 : 1;
    };
    $.ligerDefaults.Grid.sorters['string'] = $.ligerDefaults.Grid.sorters['text'];

    $.ligerDefaults.Grid.formatters['date'];

    //引用类型,数据形式表现为[id,text]
    $.ligerDefaults.Grid.formatters['ref'];

    //下拉数据窗口
    $.ligerDefaults.Grid.formatters['select'] = $.ligerDefaults.Grid.formatters['combobox'];

    //checkbox
    $.ligerDefaults.Grid.formatters['chk'] = $.ligerDefaults.Grid.formatters['checkbox'] = function (value, column) {
        var data;
        if (column.editor) data = column.editor.data || (column.editor.options ? (column.editor.options.data || undefined) : undefined);
        data = data || [{ ID: true, TEXT: '1' }, { ID: false, TEXT: '0' }];
        for (var i = 0, l = data.length; i < l; i++) {
            var item = data[i];
            if (value === item.TEXT) {
                return item.ID ? '<i class="l-checkbox l-checkbox-checked"></i>' : '<i class="l-checkbox "></i>';
            }
        }
        return value;
    };

    //扩展 percent 百分比 类型的格式化函数(0到1之间)
    $.ligerDefaults.Grid.formatters['percent'];

    //扩展 numberbox 类型的格式化函数
    $.ligerDefaults.Grid.formatters['numberbox'];

    //扩展currency类型的格式化函数
    $.ligerDefaults.Grid.formatters['currency'];

    $.ligerui.controls.Grid.prototype.setData = $.ligerui.controls.Grid.prototype._setData;
    $.ligerui.controls.Grid.prototype.setWidth = $.ligerui.controls.Grid.prototype._setWidth;
    $.ligerui.controls.Grid.prototype.setHeight = $.ligerui.controls.Grid.prototype._setHeight;
    $.ligerui.controls.Grid.prototype.enabledTotal = $.ligerui.controls.Grid.prototype.isTotalSummary;
    $.ligerui.controls.Grid.prototype.add = $.ligerui.controls.Grid.prototype.addRow;
    $.ligerui.controls.Grid.prototype.update = $.ligerui.controls.Grid.prototype.updateRow;
    $.ligerui.controls.Grid.prototype.append = $.ligerui.controls.Grid.prototype.appendRow;
    $.ligerui.controls.Grid.prototype.getSelected = $.ligerui.controls.Grid.prototype.getSelectedRow;
    $.ligerui.controls.Grid.prototype.getSelecteds = $.ligerui.controls.Grid.prototype.getSelectedRows;
    $.ligerui.controls.Grid.prototype.getCheckedRows = $.ligerui.controls.Grid.prototype.getSelectedRows;
    $.ligerui.controls.Grid.prototype.getCheckedRowObjs = $.ligerui.controls.Grid.prototype.getSelectedRowObjs;
    $.ligerui.controls.Grid.prototype.setOptions = $.ligerui.controls.Grid.prototype.set;
    $.ligerui.controls.Grid.prototype.reload = $.ligerui.controls.Grid.prototype.loadData;
    $.ligerui.controls.Grid.prototype.refreshSize = $.ligerui.controls.Grid.prototype._onResize;
    $.ligerui.controls.Grid.prototype.append = $.ligerui.controls.Grid.prototype.appendRange;
    $.ligerui.controls.Grid.prototype.showFilterHistory = $.ligerui.controls.Grid.prototype.showFilter;
    $.ligerui.controls.Grid.prototype._getCellHtml = $.ligerui.controls.Grid.prototype._getCellContent;

})(jQuery);