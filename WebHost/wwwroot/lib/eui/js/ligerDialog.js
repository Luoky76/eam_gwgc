(function ($) {
    /*
    主窗口调用子窗口的方法:
    子窗口的window对象：dialog.frame
    dialog.frame.business.具体的方法

    子窗口调用父窗口的方法:
    parent.business.具体的方法

    子窗口获取dialog对象：
    var dialog = frameElement.dialog;
    var dialogData = dialog.get('data');//获取data参数
    dialog.set('title','新标题'); //设置标题
    dialog.close();//关闭dialog
    */
    var l = $.ligerui;
    $.ligerDialog = function () {
        return l.run.call(null, "ligerDialog", arguments, { isStatic: true });
    };
    $.ligerDefaults.Dialog = {
        cls: null, //给dialog附加css class
        contentCls: null, //给dialog.content附加样式
        id: null, //给dialog附加id
        buttons: null, //按钮集合
        isDrag: true, //是否拖动
        width: null, //宽度
        height: null, //高度，默认自适应
        content: '', //内容
        target: null, //目标对象，指定它将以appendTo()的方式载入
        url: null, //目标页url，默认以iframe的方式载入
        urlParms: null, //传参
        load: false, //是否以jquery.load()的方式加载目标页的内容
        icon: null, //标题栏小图标
        type: null, //类型 warn、success、error、question
        left: null, //位置left
        top: null, //位置top
        modal: true, //是否模态对话框
        maskClose: false, //点击遮罩关闭
        data: null, //传递数据容器
        name: null, //创建iframe时 作为iframe的name和id
        isResize: false, //是否调整大小
        allowClose: true, //允许关闭
        time: null,//定时关闭关闭
        timeParmName: null, //是否给URL后面加上值为new Date().getTime()的参数，如果需要指定一个参数名即可
        isHidden: false, //关闭对话框时是否只是隐藏，还是销毁对话框
        show: true, //初始化时是否马上显示
        title: '提示', //标题
        showMax: false, //是否显示最大化按钮
        showToggle: false, //是否显示收缩窗口按钮
        showMin: false, //是否显示最小化按钮
        slide: $.browser.msie ? false : true, //是否以动画的形式显示
        fixedType: null, //在固定的位置显示, 可以设置的值有n, e, s, w, ne, se, sw, nw
        fixedGroup: false, //追加l-dialog-fixed-(fixedType)样式，产生合并的组，用于tip或者通知
        closeQuery: null, //关闭询问 返回询问字符串
        onLoaded: null, //内容加载完成事件
        onExtend: null, //开始最大化事件
        onExtended: null, //最大化完成事件
        onCollapse: null, //开始最小化事件
        onCollapseed: null, //最小化完成事件
        onContentHeightChange: null, //内容高度发生变化事件
        onClose: null, //关闭前事件
        onClosed: null, //关闭后事件
        onStopResize: null, //停止改变大小事件
        minIsHide: false   //最小化仅隐藏
    };
    $.ligerDefaults.DialogString = {
        titleMessage: '提示',                     //提示文本标题
        ok: '确定',
        yes: '是',
        no: '否',
        cancel: '取消',
        waittingMessage: '加载中...'
    };

    $.ligerMethos.Dialog = $.ligerMethos.Dialog || {};

    $.ligerDialog.open;
    $.ligerDialog.close ;
    $.ligerDialog.show;
    $.ligerDialog.hide;
    $.ligerDialog.msg;
    $.ligerDialog.tip ;
    $.ligerDialog.notification ;
    $.ligerDialog.alert;

    $.ligerDialog.confirm;
    $.ligerDialog.warning;
    $.ligerDialog.waitting;
    $.ligerDialog.closeWaitting;
    $.ligerDialog.success;
    $.ligerDialog.error;
    $.ligerDialog.warn;
    $.ligerDialog.question;

    $.ligerDialog.prompt;
})(jQuery);