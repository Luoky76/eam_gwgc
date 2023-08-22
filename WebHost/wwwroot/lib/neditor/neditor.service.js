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
        dataType: 'json',
        type: 'post',
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

/**
* 图片上传service
* @param {Object} context UploadImage对象 图片上传上下文
* @param {Object} editor  编辑器对象
* @returns imageUploadService 对象
*/
window.UEDITOR_CONFIG['imageUploadService'] = function (context, editor) {
    return {
        /**
        * 触发uploadBeforeSend事件时执行
        * 在文件上传之前触发，用来添加附带参数
        * @param {Object} object 当前上传对象
        * @param {Object} data 默认的上传参数，可以扩展此对象来控制上传参数
        * @param {Object} headers 可以扩展此对象来控制上传头部
        * @returns 上传参数对象
        */
        setFormData: function (object, data, headers) {
            var jqXHR = {
                setRequestHeader: function (header, value) {
                    headers[header] = value;
                }
            };
            window.setGksybToken(jqXHR);
            return data;
        }
    }
};

/**
* 视频上传service
* @param {Object} context UploadVideo对象 视频上传上下文
* @param {Object} editor  编辑器对象
* @returns videoUploadService 对象
*/
window.UEDITOR_CONFIG['videoUploadService'] = function (context, editor) {
    return {
        /**
        * 触发uploadBeforeSend事件时执行
        * 在文件上传之前触发，用来添加附带参数
        * @param {Object} object 当前上传对象
        * @param {Object} data 默认的上传参数，可以扩展此对象来控制上传参数
        * @param {Object} headers 可以扩展此对象来控制上传头部
        * @returns 上传参数对象
        */
        setFormData: function (object, data, headers) {
            var jqXHR = {
                setRequestHeader: function (header, value) {
                    headers[header] = value;
                }
            };
            window.setGksybToken(jqXHR);
            return data;
        }
    }
};

/**
* 涂鸦上传service
* @param {Object} context scrawlObj对象
* @param {Object} editor  编辑器对象
* @returns scrawlUploadService 对象
*/
window.UEDITOR_CONFIG['scrawlUploadService'] = function (context, editor) {
    return scrawlUploadService = {
        /**
        * 点击涂鸦模态框确认按钮时触发
        * 上传涂鸦图片
        * @param {Object} file 涂鸦canvas生成的图片
        * @param {Object} base64 涂鸦canvas生成的base64
        * @param {Function} success 上传成功回调函数,回传上传成功的response对象
        * @param {Function} fail 上传失败回调函数,回传上传失败的response对象
        */

        /**
        * 上传成功的response对象必须为以下两个属性赋值
        * 
        * 上传接口返回的response成功状态条件 {Boolean} (比如: res.code == 200)
        * res.responseSuccess = res.code == 200;
        * 
        * 指定上传接口返回的response中涂鸦图片路径的字段，默认为 url 
        * res.videoSrcField = 'url';
        */
        uploadScraw: function (file, base64, success, fail) {
            /* 模拟上传操作 */
//            var formData = new FormData();
//            formData.append('file', file, file.name);

//            $.ajax({
//                url: editor.getActionUrl(editor.getOpt('scrawlActionName')),
//                type: 'POST',
//                data: formData
//            }).done(function (res) {
//                var res = JSON.parse(res);

//                /* 上传接口返回的response成功状态条件 (比如: res.code == 200) */
//                res.responseSuccess = res.code == 200;

//                /* 指定上传接口返回的response中涂鸦图片路径的字段，默认为 url 
//                * 如果涂鸦图片路径字段不是res的属性，可以写成 对象.属性 的方式，例如：data.url
//                */
//                res.scrawlSrcField = 'url';

//                /* 上传成功 */
//                success.call(context, res);
//            }).fail(function (err) {
//                /* 上传失败 */
//                fail.call(context, err);
//            });
        }
    }
}

/**
* 附件上传service
* @param {Object} context UploadFile对象 附件上传上下文
* @param {Object} editor  编辑器对象
* @returns fileUploadService 对象
*/
window.UEDITOR_CONFIG['fileUploadService'] = function (context, editor) {
    return {
        /**
        * 触发uploadBeforeSend事件时执行
        * 在文件上传之前触发，用来添加附带参数
        * @param {Object} object 当前上传对象
        * @param {Object} data 默认的上传参数，可以扩展此对象来控制上传参数
        * @param {Object} headers 可以扩展此对象来控制上传头部
        * @returns 上传参数对象
        */
        setFormData: function (object, data, headers) {
            var jqXHR = {
                setRequestHeader: function (header, value) {
                    headers[header] = value;
                }
            };
            window.setGksybToken(jqXHR);
            return data;
        }
    }
};