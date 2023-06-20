(function ($) {
    var l = $.ligerui;
    $.ligerDialog = function () {
        return l.run.call(null, "ligerDialog", arguments, { isStatic: true });
    };
    $.ligerDefaults.Dialog = {
        cls: null,       //给dialog附加css class
        contentCls: null,
        id: null,        //给dialog附加id
        buttons: null, //按钮集合
        isDrag: true,   //是否拖动
        width: null,     //宽度
        height: null,   //高度，默认自适应
        content: '',    //内容
        target: null,   //目标对象，指定它将以appendTo()的方式载入
        url: null,      //目标页url，默认以iframe的方式载入
        urlParms: null,     //传参
        load: false,     //是否以load()的方式加载目标页的内容
        icon: null,      //标题栏小图标
        type: null,   //类型 warn、success、error、question
        left: null,     //位置left
        top: null,      //位置top
        modal: true,    //是否模态对话框
        maskClose: false, //点击遮罩关闭
        data: null,     //传递数据容器
        name: null,     //创建iframe时 作为iframe的name和id
        isResize: false, // 是否调整大小
        allowClose: true, //允许关闭
        opener: null,
        timeParmName: null,  //是否给URL后面加上值为new Date().getTime()的参数，如果需要指定一个参数名即可
        closeWhenEnter: null, //回车时是否关闭dialog
        isHidden: false,        //关闭对话框时是否只是隐藏，还是销毁对话框
        show: true,          //初始化时是否马上显示
        title: '提示',        //头部
        showMax: false,                             //是否显示最大化按钮
        showToggle: false,                          //是否显示收缩窗口按钮
        showMin: false,                             //是否显示最小化按钮
        slide: $.browser.msie ? false : true,        //是否以动画的形式显示
        fixedType: null,            //在固定的位置显示, 可以设置的值有n, e, s, w, ne, se, sw, nw
        fixedGroup: false,             //追加到l-dialog-fixed的组里面
        onLoaded: null,
        onExtend: null,
        onExtended: null,
        onCollapse: null,
        onCollapseed: null,
        onContentHeightChange: null,
        onClose: null,
        onClosed: null,
        onStopResize: null,
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