(function ($) {
    $.ligerDefaults.Upload = {
        type: 'card' //类型 text,card
        , itemWidth: null //单项宽度 不设置默认取宽度
        , width: 100 //宽度
        , height: 100 //高度
        , showName: true //显示上传文件名称
        , title: "<p>支持拖拽上传</p><p>格式：{extensions}</p>" //上传文字描述
        , extensions: "jpg,jpeg,png,gif,bmp"//允许上传的文件后缀
        , mimeTypes: 'image/*' //允许上传的文件类型
        , compress: {//压缩格式
            force: true //强制压缩，不管文件是否超过fileSizeLimit
            , width: 1920
            , height: null
            , quality: 90
        }
        , url: null //上传地址
        , ajaxOptions: null //ajax扩展属性
        , name: "formFile" //表单域名称
        , data: {} //请求上传的额外参数
        , initTrigger: false //初始化触发onChangeValue
        , split: ","//分隔符
        , fileSizeLimit: 3 * 1024 * 1024 //文件限制大小，单位字节，默认3M
        , fileNumLimit: 0 //最大上传的文件数，默认不限制
        , multiple: false //是否允许多文件上传
        , onSuccess: null //上传后回调
    };
    $.ligerDefaults.UploadString = {
        typeDeniedText: '只能上传文件类型为{extensions}的文件'
        , numLimitText: '最多只能上传{fileNumLimit}个文件'
        , sizeLimitText: '文件大小不能超过{fileSizeLimit}M'
        , loadingText: '上传中...'
        , fieIcon: "data:image/png;base64,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"
    };
})(jQuery);