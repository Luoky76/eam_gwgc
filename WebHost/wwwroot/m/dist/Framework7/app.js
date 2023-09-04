; (function () {
    'use strict';
    var urlBase = "/";
    for (var i = 0, l = document.scripts.length; i < l; i++) {
        var src = document.scripts[i].src;
        var index = src.indexOf("m/dist/Framework7/app.js");
        if (index > 0) {
            urlBase = src.substring(0, index);
            var host = "//" + location.host;
            index = urlBase.indexOf(host);
            if (index > 0) urlBase = urlBase.substring((index + host.length));
            break;
        }
    }
    //全局配置
    window.gksybConfigs = {
        urlBase: urlBase + "m/v/", // url访问路径
        apiBase: urlBase, // api 接口的访问路径
        getUrl: function (url) {
            if (url.indexOf("http") < 0) { //url处理
                if (this.apiBase) {
                    url = url.replace(/\.\.\//g, "");
                    if (url.indexOf(this.urlBase) === 0) return url;
                    if (url.indexOf(this.apiBase) === 0) return url;
                }
                url = this.apiBase + url;
            }
            return url;
        }
    };

    //会话变量
    if (!window.session) {
        var sessionKey = "GksybData";
        Object.defineProperty(window, 'session', {
            get: function () {
                var data = window.localStorage.getItem(sessionKey);
                if (data) return JSON.parse(data);
                return data || {};
            },
            set: function (val) {
                if (val && typeof val !== "string") {
                    val = JSON.stringify(val);
                }
                if (val) {
                    window.localStorage.setItem(sessionKey, val);
                } else {
                    window.localStorage.removeItem(sessionKey);
                }
            }
        });
    }
    //票据
    if (!window.ticket) {
        var ticketKey = "GksybTicket";
        Object.defineProperty(window, 'ticket', {
            get: function () {
                return window.localStorage.getItem(ticketKey) || "";
            },
            set: function (val) {
                if (val) {
                    window.localStorage.setItem(ticketKey, val);
                } else {
                    window.localStorage.removeItem(ticketKey);
                }
            }
        });
    }
    //顶层窗口
    if (!window.topWindow) {
        Object.defineProperty(window, 'topWindow', {
            get: function () {//获取不跨域的顶层窗口
                var parentWindow = window;
                try {
                    for (var i = 0; i < 10; i++) {
                        if (parentWindow.parent.location.href) {
                            parentWindow = parentWindow.parent;
                        }
                    }
                } catch (err) {
                }
                return parentWindow;
            }
        });
    }
    //全局方法扩展
    Framework7.utils.extend(window, {
        $: Dom7,
        $$: Dom7,
        androidCallBack: function (method, data) { //安卓回调
            if (window[method]) {
                window[method](data);
            }
        },
        generateJsToken: function (jqXHR, opt) {//js票据 eval用到jqXHR
            opt = opt || { jsToken: "JsToken" };
            var tokenOptions = Framework7.utils.extend(true, {
                noGlobal: true,
                url: "Auth/JsToken",
                async: false,
                data: {
                    key: (opt.jsToken === true) ? opt.url : opt.jsToken
                },
                dataType: "text",
                type: 'post',
                success: function (result) {
                    eval(result);
                },
                error: function () { }
            }, opt.tokenOptions);
            app.request(tokenOptions);
        },
        setGksybToken: function (jqXHR) {//token验证
            if (window.session.Token) jqXHR.setRequestHeader("GKSYBTOKEN", window.session.Token);
        },
        _toLogin: function () {
            var loginUrl = gksybConfigs.urlBase + "login.html?FromUrl=" + encodeURIComponent(topWindow.location.href);
            topWindow.location.href = loginUrl;
            return;
        },
        innerDialogTip: function (msg, _toLogin) {
            if (_toLogin === true) {
                window._toLogin();
                return;
            }
            app.dialog.error(msg, function () {
                window._toLogin();
            });
        },
        isInWeixin: function () {//判断微信浏览器 如不是微信应用直接返回false
            //return false;
            return (window.navigator.userAgent || "").indexOf("MicroMessenger") >= 0;
        },
        weixinOauth: function () {
            var redirectUrl = location.origin + gksybConfigs.urlBase + "weixin/oauth.html";
            redirectUrl += "?FromUrl=" + encodeURIComponent(topWindow.location.href);
            redirectUrl = encodeURIComponent(redirectUrl);
            Framework7.ajax({
                async: false,
                noGlobal: true,
                url: 'weixin/authorizeurl',
                headers: {
                    redirectUrl: redirectUrl
                },
                success: function (data) {
                    topWindow.location.href = data;
                }
            });
        },
        refreshGksybToken: function (callback, _toLogin) {
            var me = this;
            if (me.isInWeixin()) {//微信浏览器
                me.weixinOauth();
                return;
            }
            var innerTip = window.innerDialogTip;
            Framework7.ajax({
                async: false,
                noGlobal: true,
                url: 'Auth/RefreshToken',
                headers: {
                    ticket: window.ticket
                },
                success: function (data) {
                    window.ticket = data.Ticket;
                    delete data.Ticket;
                    window.session = data;
                    callback();
                },
                error: function () {
                    innerTip("登录信息已失效，请重新登录。", _toLogin);
                }
            });
        }
    });

    var preloaderQueue = 0;
    Framework7.request.setup({
        contentType: "application/json",
        beforeOpen: function (jqXHR, opt) {
            delete jqXHR.orginOpt;
            jqXHR.orginOpt = Framework7.utils.extend(true, {}, opt);
            opt.url = gksybConfigs.getUrl(opt.url || "");
            var contentType = (opt.contentType || "").toLocaleLowerCase();
            var isJson = contentType.indexOf("application/json") >= 0;
            var dataType = Object.prototype.toString.apply(opt.data);
            if (!(opt.noGlobal || opt.noGlobalBeforeOpen)) {
                var bolContinue = true;
                if (opt.data && opt.data.skipEncrypt) bolContinue = false;
                if (bolContinue) {
                    var encryptPara = {
                        "view": new RegExp("view(]?)$", "i"),
                        "idfield": new RegExp("idfield(]?)$", "i"),
                        "textfield": new RegExp("textfield(]?)$", "i"),
                        "valuefield": new RegExp("valuefield(]?)$", "i"),
                        "encrpyCls": new RegExp("columns(]?)$", "i"),
                        "encrpyCondition": new RegExp("where(]?)$", "i"),
                        "sortname": new RegExp("sortname(]?)$", "i"),
                        "sortorder": new RegExp("sortorder(]?)$", "i"),
                        "groupby": new RegExp("groupby(]?)$", "i")
                    };
                    var changeName = {
                        "encrpyCls": "columns",
                        "encrpyCondition": "where"
                    };
                    if (isJson && dataType === "[object String]") {
                        try {
                            opt.data = JSON.parse(opt.data);
                            dataType = Object.prototype.toString.apply(opt.data);
                        } catch (e) { }
                    }
                    if (dataType === "[object Object]") {
                        for (var para in opt.data) {
                            for (var name in encryptPara) {
                                if ((encryptPara[name]).test(para)) {
                                    var value = opt.data[para];
                                    value = encrypt(value);
                                    if (changeName[name]) {
                                        delete opt.data[para];
                                        para = name;
                                    }
                                    opt.data[para] = value;
                                    break;
                                }
                            }
                        }
                    }
                    else if (dataType === "[object Array]") {
                        for (var i in opt.data) {
                            var item = opt.data[i];
                            if (!item.name || !item.value) continue;
                            for (var name in encryptPara) {
                                if ((encryptPara[name]).test(item.name)) {
                                    item.value = encrypt(item.value);
                                    if (changeName[name]) item.name = name;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        },
        beforeSend: function (jqXHR, opt) {
            if (opt.jsToken) { //防止机器人提交
                window.generateJsToken(jqXHR, opt);
                delete opt.tokenOptions;
            }
            window.setGksybToken(jqXHR);
            if (opt.noGlobal) return;
            if (opt.noGlobalBeforeSend) return;
            app.preloader.show();
            if (!preloaderQueue) preloaderQueue = 0;
            preloaderQueue++;
            jqXHR._orginOnload = jqXHR.onload;
            jqXHR._orginOnerror = jqXHR.onerror;
            jqXHR._orginOonabort = jqXHR.onabort;
            jqXHR.onload = function onload() {
                if (!(--preloaderQueue)) app.preloader.hide()
                if (jqXHR._orginOnload) jqXHR._orginOnload();
            }
            jqXHR.onerror = function () {
                if (!(--preloaderQueue)) app.preloader.hide()
                if (jqXHR._orginOnerror) jqXHR._orginOnerror();
            }
            jqXHR.onabort = function () {
                if (!(--preloaderQueue)) app.preloader.hide()
                if (jqXHR._orginOonabort) jqXHR._orginOonabort();
            }
        },
        error: function (jqXHR, status) {
            var opt = jqXHR.requestParameters;
            if (opt.noGlobal) return;
            if (opt.noGlobalError) return;
            if (jqXHR.status === 999) {
                if ((jqXHR.orginOpt || jqXHR.requestParameters).redo === true) {
                    app.dialog.error("登录信息已失效，请重新登录。");
                    return;
                }
                (jqXHR.orginOpt || jqXHR.requestParameters).redo = true;
                window.refreshGksybToken(function () {
                    Framework7.request(jqXHR.orginOpt || jqXHR.requestParameters); //登录后重发请求
                }, opt._toLogin);
            } else {
                app.dialog.error('请求数据出错,页面即将跳转!<br/>原因为：' + (jqXHR.responseText.substring(0, 20) || "") + "<br/>错误码:" + (jqXHR.status || ""), "错误", function () {
                    //location.reload();
                });
            }
        }
    });
    //ajax请求
    Framework7.ajax = function (options) {
        options.contentType = options.contentType || "application/json";
        options.method = options.method || options.type || 'post';
        options.dataType = options.dataType || 'json';

        var successInner = options.success;
        var errorInner = options.error;
        delete options.error;
        options.success = successInner ? function (result) {
            if (!result) return;
            if (result.IsError) {
                if (errorInner) errorInner(result.Message, result.Data);
                else {
                    app.dialog.error(result.Message);
                }
            } else {
                if (successInner) successInner(result.Data, result.Message);
            }
        } : null;
        return this.request(options).then(function () { }, function () { });
    };

    var app = new Framework7({
        id: "gksyb.weiapp",
        name: "weiapp",
        el: "#app",
        theme: "md",
        routes: [],
        autocomplete: {
            popupCloseLinkText: "返回",
            pageBackLinkText: "关闭",
            searchbarPlaceholder: "请输入...",
            searchbarDisableText: "取消",
            notFoundText: "无记录"
        },
        calendar: {
            dateFormat: "yyyy-mm-dd",
            timePickerPlaceholder: "时间选择",
            toolbarCloseText: "确定",
            headerPlaceholder: "请选择日期"
        },
        dialog: {
            title: '微平台',
            buttonOk: '确定',
            buttonCancel: '取消',
            usernamePlaceholder: "用户名",
            passwordPlaceholder: "密码",
            preloaderTitle: '加载中...',
            progressTitle: '加载中...'
        },
        picker: {
            toolbarCloseText: "确定"
        },
        photoBrowser: {
            pageBackLinkText: "返回",
            popupCloseLinkText: "关闭",
            navbarOfText: "/"
        },
        smartSelect: {
            openIn: "popup", //如果用page会产生触发change时.page-current指向弹出页，只能用.page[data-name=名称]父页面
            pageBackLinkText: "返回",
            popupCloseLinkText: "关闭",
            sheetCloseLinkText: "确定",
            searchbarPlaceholder: "请输入...",
            searchbarDisableText: "取消",
            closeOnSelect: true
        },
        toast: {
            closeButtonText: "确定"
        },
        touch: {
            fastClicks: false
        },
        view: {
            stackPages: true,
        },
        on: {
            calendarOpen: function (calendar) {
                $(calendar.$el).find('.calendar-close').off('click').on('click', function () {
                    if (calendar.params.rangePicker && calendar.value.length !== 2) {
                        app.toast.create({
                            text: '请选择时间范围',
                            position: 'top',
                            closeTimeout: 2000
                        }).open();
                        return;
                    }
                    calendar.close();
                });
            },
            pageMounted: function (page) {
                var scriptEl = "#scriptMounted";
                if ($$(page.router.tempDom).find(scriptEl).length > 0) {
                    app.methods.initPage(page, scriptEl);
                }
            },
            pageAfterIn: function (page) {
                if ($$(page.router.tempDom).find("#scriptMounted").length > 0) {
                    return;
                }
                app.methods.initPage(page, "#scriptBlock");
            },
            routeChanged: function (newRoute, previousRoute, router) { //为路由加入routeChanged函数触发 单个路由无此事件
                var routeChanged = newRoute.route.on ? newRoute.route.on.routeChanged : null;
                if (routeChanged instanceof Function) {
                    routeChanged(newRoute, previousRoute, router);
                }
            }
        }
    });
    //ajax
    app.ajax = Framework7.ajax;
    //错误提醒
    app.dialog.error = function () {
        if (arguments.length > 0) arguments[0] = '<span class="error">' + arguments[0] + '</span>';
        return app.dialog.alert.apply(this, arguments);
    };
    app.methods = {
        //固定表格
        frozenTable: function ($table, num, isRowFrozen) {
            $table = $$($table);
            if (!$table || !$table.length) {
                $table = $$(".page-current").find("table").filter(function (i, el) {
                    return !$$(el).parents(".frozen-column").length;
                });
            }
            if (!$table || !$table.length) return;
            if ($table.find("table").length) $table = $table.find("table");
            $table.parents(".frozen-wrap").find(".frozen-child").remove();
            num = num || ($table.find("[frozen]").index() + 1);
            if (isRowFrozen === undefined) isRowFrozen = $table.attr("frozen") !== null;
            if (!num && (isRowFrozen !== true)) return;
            var $wrap = $table.parents(".frozen-wrap");
            if (!$wrap.length) {
                $wrap = $$('<div class="frozen-wrap" style="position:relative;"><div id="table-wrap-orgin" name="table-wrap-orgin" style="width:100%;overflow-x:auto;-webkit-overflow-scrolling: touch;"></div></div>');
                $wrap.insertBefore($table);
                $table.prependTo($wrap.find("div"));
                var $dataTable = $table.parents(".data-table");
                var tableHeight = $dataTable.parent().height() - $dataTable.offset().top - parseInt($dataTable.parent().css("padding-bottom")) - ($dataTable.outerHeight(true) - $dataTable.height(true)) / 2;
                $dataTable.css("max-height", tableHeight + "px");
            }

            function f_checkbox_change(obj) {
                var $el = $$(obj).parents("tr");
                setTimeout(function () {
                    var status = $el.hasClass("data-table-row-selected");
                    var index = $el.index();
                    var $obj = $table.find('tbody .checkbox-cell input[type="checkbox"]').eq(index);
                    if ($obj.parents("tr").hasClass("data-table-row-selected") !== status) {
                        $obj.click();
                    }
                }, 150);
            }

            function f_sort(obj) {
                var $cellEl = $$(obj);
                var isActive = $cellEl.hasClass('sortable-cell-active');
                var currentSort = $cellEl.hasClass('sortable-desc') ? 'desc' : 'asc';
                var newSort;
                if (isActive) {
                    newSort = currentSort === 'desc' ? 'asc' : 'desc';
                    $cellEl.removeClass('sortable-desc sortable-asc').addClass(("sortable-" + newSort));
                } else {
                    $cellEl.parent().find('thead .sortable-cell-active').removeClass('sortable-cell-active');
                    $cellEl.addClass('sortable-cell-active');
                    newSort = currentSort;
                }
                var index = $cellEl.index();
                setTimeout(function () {
                    $table.find("th").eq(index).click();
                }, 150);
            }
            var frozenWidth = ($table.find("tr th").eq(num || 0).offset().left - $table.find("tr th").eq(0).offset().left) + "px";
            var frozenHeight = $table.find("tr th").outerHeight(true) + "px";
            if (isRowFrozen === true) {
                var $divRow = $$('<div class="frozen-row frozen-child"></div>');
                $divRow.html($table.prop("outerHTML"));
                $divRow.css({
                    "position": "fixed",
                    "overflow": "hidden",
                    "background-color": "white",
                    "z-index": "99990",
                    "box-shadow": "1px -1px 8px 1px #d3d4d6",
                    "width": "100%",
                    "height": frozenHeight
                });
                $table.parent().touchmove(function (a, b, c) {
                    var left = $$(this).scrollLeft();
                    $divRow.scrollLeft(left);
                });
                $divRow.prependTo($wrap);
                var $forzenRow = $divRow.find("table").attr({
                    "id": ($table.attr("id") || "") + "_frozenRow",
                    "name": ($table.attr("name") || "") + "_frozenRow",
                });
                $forzenRow.find('tbody .checkbox-cell input[type="checkbox"]').off('change').on('change', function () {
                    f_checkbox_change(this);
                });
                $forzenRow.find('thead .sortable-cell').on('click', function () {
                    f_sort(this);
                });
                $divRow.prependTo($wrap);
            }
            if (num) {
                var $divColumn = $$('<div class="frozen-column frozen-child"></div>');
                $divColumn.html($table.prop("outerHTML"));
                $divColumn.css({
                    "position": "absolute",
                    "overflow-x": "hidden",
                    "background-color": "white",
                    "z-index": "99991",
                    "box-shadow": "1px -1px 8px 1px #d3d4d6",
                    "width": frozenWidth
                });
                var $forzenColumn = $divColumn.find("table").attr({
                    "id": ($table.attr("id") || "") + "_frozenRow",
                    "name": ($table.attr("name") || "") + "_frozenRow",
                }).css("width", $table.width() + "px");
                $forzenColumn.find('tbody .checkbox-cell input[type="checkbox"]').off('change').on('change', function () {
                    f_checkbox_change(this);
                });
                $forzenColumn.find('thead .sortable-cell').on('click', function () {
                    f_sort(this);
                });
                $divColumn.prependTo($wrap);
            }
            if (num && isRowFrozen === true) {
                var $divRowColumn = $$('<div class="frozen-row-column frozen-child"></div>');
                $divRowColumn.html($table.prop("outerHTML"));
                $divRowColumn.css({
                    "position": "fixed",
                    "overflow": "hidden",
                    "background-color": "white",
                    "z-index": "99999",
                    "box-shadow": "1px -1px 8px 1px #d3d4d6",
                    "width": frozenWidth,
                    "height": frozenHeight
                });
                var $forzenRowColumn = $divRowColumn.find("table").attr({
                    "id": ($table.attr("id") || "") + "_frozenRow",
                    "name": ($table.attr("name") || "") + "_frozenRow",
                }).css("width", $table.width() + "px");
                $forzenRowColumn.find('tbody .checkbox-cell input[type="checkbox"]').off('change').on('change', function () {
                    f_checkbox_change(this);
                });
                $forzenRowColumn.find('thead .sortable-cell').on('click', function () {
                    f_sort(this);
                });
                $divRowColumn.prependTo($wrap);
            }
        },
        //获取排序
        getTableSort: function ($table) {
            $table = $$($table);
            if (!$table || !$table.length) {
                $table = $$(".page-current").find("table").filter(function (i, el) {
                    return !$$(el).parents(".frozen-column").length;
                });
            }
            if (!$table || !$table.length) return;
            if ($table.find("table").length) $table = $table.find("table");
            var $active = $table.find(".sortable-cell-active");
            if (!$active || !$active.length) return "";
            var ss = ($active.attr("dbname") || $active.text()) + " " + ($active.hasClass("sortable-desc") ? "DESC" : "ASC");
            return ss;
        },
        //初始化排序
        initTableSort: function ($table, callback) {
            $table = $$($table);
            if (!$table || !$table.length) {
                $table = $$(".page-current").find("table").filter(function (i, el) {
                    return !$$(el).parents(".frozen-column").length;
                });
            }
            if (!$table || !$table.length) return;
            if ($table.find("table").length) $table = $table.find("table");
            if ($table.find(".sortable-cell").length < 1) {
                $table.find("th").filter(function (i, el) {
                    return !$$(el).hasClass("checkbox-cell");
                }).addClass("sortable-cell");
            }
            var dataTable = app.dataTable.get($table.parents(".data-table"));
            if (!dataTable) dataTable = app.dataTable.create({
                "el": $table.parents(".data-table")
            });
            dataTable.off("dataTableSort").on("dataTableSort", function (a, b, c) {
                if (callback) callback(app.methods.getTableSort($table));
            });
        },
        //验证输入
        validateInputs: function (el) {
            app.input.validateInputs(el);
            if ($$(el).find(".input-invalid").length) {
                return false;
            }
            return true;
        },
        //转向url
        toUrl: function (url, options) {
            var indexUrl = gksybConfigs.urlBase + "index.html";
            if (url === "/" || url === "/Index/") {
                location.replace(indexUrl);
                return;
            }
            var pathname = location.pathname;
            pathname = pathname.toLocaleLowerCase();
            if (pathname.indexOf(indexUrl) < 0) {
                if (url === "back") {
                    location.href = indexUrl;
                } else {
                    location.href = indexUrl + "#!/" + url.replace(/\/|\\/g, "") + "/";
                }
            } else {
                if (url === "back") {
                    mainView.router.back();
                    document.title = app.lastTitle;
                } else {
                    mainView.router.navigate(url, options);
                }
            }
        },
        android: {
            getIMEI: function () {
                if (window.android) {
                    window.android.JsExcute("IMEI", ''); //返回调用 window["imeiBack"]
                }
            }
        },
        //获取OpenID
        getOpenID: function (callback, url) {
            var openid = window.session.Openid;
            if (openid) {
                if (callback) callback(openid);
                return;
            }
            Framework7.ajax({
                noGlobal: true,
                url: 'Weixin/Openid',
                success: function (data) {
                    openid = data;
                    if (callback) callback(openid);
                }
            });
        },
        //初始化微信服务JSSDK
        initWX: function (callback) {
            var inner = function (model) {
                wx.config({
                    debug: false, // 开启调试模式,调用的所有api的返回值会在客户端alert出来，若要查看传入的参数，可以在pc端打开，参数信息会通过log打出，仅在pc端时才会打印。
                    appId: model.AppId, // 必填，公众号的唯一标识
                    timestamp: model.Timestamp, // 必填，生成签名的时间戳
                    nonceStr: model.NonceStr, // 必填，生成签名的随机串
                    signature: model.Signature, // 必填，签名
                    jsApiList: [
                        'checkJsApi',
                        'chooseImage',//拍照或从手机相册中选图接口
                        'previewImage',//预览图片接口
                        'uploadImage',//上传图片接口
                        'downloadImage',//下载图片接口
                        'getNetworkType',//获取网络状态接口
                        'openLocation',//使用微信内置地图查看位置接口
                        'getLocation',//获取地理位置接口
                        'closeWindow',//关闭当前网页窗口接口
                        'scanQRCode',//调起微信扫一扫接口
                        'chooseWXPay',//发起一个微信支付请求

                        'startRecord',//开始录音接口
                        'stopRecord',//停止录音接口
                        'onVoiceRecordEnd',//监听录音自动停止接口
                        'playVoice',//播放语音接口
                        'pauseVoice',//暂停播放接口
                        'stopVoice',//停止播放接口
                        'onVoicePlayEnd',//监听语音播放完毕接口
                        'uploadVoice',//上传语音接口
                        'downloadVoice',//下载语音接口
                        'translateVoice',//识别音频并返回识别结果接口

                        //'updateAppMessageShareData',//自定义“分享给朋友”及“分享到QQ”按钮的分享内容
                        //'updateTimelineShareData',//自定义“分享到朋友圈”及“分享到QQ空间”按钮的分享内容
                        //'onMenuShareWeibo',//获取“分享到腾讯微博”按钮点击状态及自定义分享内容接口
                        //'onMenuShareQZone',//获取“分享到QQ空间”按钮点击状态及自定义分享内容接口
                        //'hideOptionMenu',
                        //'showOptionMenu',
                        //'hideMenuItems',//批量隐藏功能按钮接口
                        //'showMenuItems',//批量显示功能按钮接口
                        //'hideAllNonBaseMenuItem',//隐藏所有非基础按钮接口
                        //'showAllNonBaseMenuItem',//显示所有功能按钮接口
                        //'openProductSpecificView',//跳转微信商品页接口
                        //'addCard',//批量添加卡券接口
                        //'chooseCard',//拉取适用卡券列表并获取用户选择信息
                        //'openCard',//查看微信卡包中的卡券接口
                    ] // 必填，需要使用的JS接口列表，所有JS接口列表见附录2。详见：http://mp.weixin.qq.com/wiki/7/aaa137b55fb2e0456bf8dd9148dd613f.html
                });
                if (callback) wx.ready(callback);
            }
            var signUrl = (location.href || "");
            var index = signUrl.indexOf("#");
            if (index >= 0) {
                signUrl = signUrl.substring(0, index);
            }
            if (app.WXSignUrl !== signUrl) {
                Framework7.ajax({
                    noGlobal: true,
                    url: 'Weixin/JsSDK',
                    data: { url: signUrl },
                    success: function (data) {
                        app.WXSignUrl = signUrl;
                        app.WXJSSDK = data;
                        inner(app.WXJSSDK);
                    }
                });
            } else {
                inner(app.WXJSSDK);
            }
        },
        //初始化主页
        initMainView: function (options) {
            options = options || {};
            if (options.main !== false) options.main = true;
            var mainId = "view_main", viewHistory = "f7router-" + mainId + "-history", url = location.href, route;
            var browserHistorySeparator = options.browserHistorySeparator || app.params.view.browserHistorySeparator;
            if (browserHistorySeparator.length > 0 && url.indexOf(browserHistorySeparator) >= 0) {
                //直接访问路由 调整历史记录，调整localStorage
                var urls = url.split(browserHistorySeparator), homeRoute = options.url || "/";
                route = urls[1];
                if (!(history.state && history.state[mainId])) {
                    history.replaceState({ [mainId]: { url: homeRoute } }, '', urls[0]);
                    history.pushState({}, '', url);
                    window.localStorage[viewHistory] = JSON.stringify([homeRoute, route]);
                }
            }
            else {
                delete window.localStorage[viewHistory];
            }
            var oldStorage = window.localStorage[viewHistory];
            if (options.browserHistory !== false) {
                options.browserHistory = true;
                window.mainView = window.app.views.create('.view-main', options);
            }
            if (!window.mainView) {
                options.browserHistory = false;
                window.mainView = window.app.views.create('.view-main', options);
            }
            app.routes = window.mainView.routes;
            if (route) {
                //直接访问路由 路由，localStorage处理
                Framework7.history.replace(window.mainView.id, {
                    url: route
                }, url);
                setTimeout(function () {
                    window.localStorage[viewHistory] = oldStorage;
                }, 30);
            }
        },
        //初始化页面
        initPage: function (page, scriptEl) {
            if (!app._params) app._params = {};
            var attrs = [];
            for (var attr in app._params) {
                if (attr.toString().indexOf("__") === 0) {
                    attrs.push(attr);
                }
            }
            for (var i = 0, l = attrs.length; i < l; i++) {
                delete app._params[attrs[i]];
            }
            app._params.page = page;
            var $el = $$(page.router.tempDom);
            var lastTitle = app.lastTitle;
            app.lastTitle = document.title;
            if (page.direction !== "backward") { //后退按钮不触发执行js
                document.title = page.route.route.title || lastTitle || document.title;
                var script = $el.find(scriptEl).html();
                if (script) {
                    var name = 'app.methods["router_' + (page.router.currentRoute.name || page.router.currentRoute.path.replace(/\//g, '')) + '"]';
                    script = ";try{" + name + " = function(page){" + script + "};" + name + "(app._params.page);" + name + " = null;delete " + name + ";}catch (e) {console.log(JSON.stringify(e));}";
                    window.eval(script);
                    //scriptEl = document.createElement('script');
                    //scriptEl.innerHTML = script;
                    //$('head').append(scriptEl);
                    //$(scriptEl).remove();
                }
            } else {
                document.title = page.route.route.title || $el.find("title").html() || document.title;
            }
        }
    };
    // app.form.fillFromData(page.pageFrom.$el.find("#my-form"),{字段:"值"});//为前一个页面赋值
    app.form.orginConvertToData = app.form.convertToData;
    app.form.convertToData = function (formEl) { //去除两边空格
        var formData = app.form.orginConvertToData(formEl);
        var $formEl = $$(formEl);
        if ($formEl.hasClass("trim")) {
            for (var name in formData) {
                var val = formData[name];
                if (val && Object.prototype.toString.apply(val) === '[object String]') {
                    val = val.trim();
                    formData[name] = val;
                }
            }
        } else {
            $formEl.find('[trim]').each(function (inputEl) {
                var name = $$(inputEl).attr('name');
                var val = formData[name];
                if (val && Object.prototype.toString.apply(val) === '[object String]') {
                    val = val.trim();
                    formData[name] = val;
                }
            });
        }
        if ($formEl.hasClass("upper")) {
            for (var name in formData) {
                var val = formData[name];
                if (val && Object.prototype.toString.apply(val) === '[object String]') {
                    val = val.toUpperCase();
                    formData[name] = val;
                }
            }
        } else {
            $formEl.find('[upper]').each(function (inputEl) {
                var name = $$(inputEl).attr('name');
                var val = formData[name];
                if (val && Object.prototype.toString.apply(val) === '[object String]') {
                    val = val.toUpperCase();
                    formData[name] = val;
                }
            });
        }
        return formData;
    };

    //自动生成详情编辑页
    app.CreatePopupContentHtml = function (dict) {
        var ul = document.createElement('ul');
        for (var i = 0; i < dict.length; i++) {

            if (dict[i].isPrimaryKey) {
                continue;
            }

            if (dict[i].isShow == false) {
                var input = document.createElement('input');
                input.type = "hidden"
                input.name = dict[i].name || "";
                input.id = dict[i].name || "";
                ul.appendChild(input);
                continue;
            }

            var li = document.createElement('li');
            var iteminner = document.createElement('div');
            var itemtitle = document.createElement('div');
            var itemafter = document.createElement('div');
            var input = document.createElement('input');
            var select = document.createElement('select');
            var itemsmart = document.createElement('a');
            var iselect = null;
            if (dict[i].hasOwnProperty("Option")) {
                select.name = dict[i].name || "";
                select.id = dict[i].name || "";

                select.disabled = dict[i].readOnly;
                select.options.add(new Option("请选择", "-99")); //这个兼容IE与firefox
                dict[i].Option.forEach(x => {
                    select.options.add(new Option(x.TEXT, x.ID));
                });
                select.setAttribute("type", "select");
                itemsmart.href = "#";
                itemsmart.className = "item-link smart-select smart-select-init " + dict[i].smartSelectClass ?? dict[i].name;
                if (dict[i].Searched) {
                    itemsmart.setAttribute("data-open-in", "popup");
                    itemsmart.setAttribute("data-searchbar", "true");
                } else {
                    itemsmart.setAttribute("data-open-in", "sheet");
                }
                itemsmart.setAttribute("data-virtual-list", "true");
                itemsmart.setAttribute("data-page-back-link-text", "Go back");
                iselect = true;
            }
            else {
                input.setAttribute("style", "text-align: right;font-size: 14px;color: rgba(0, 0, 0, 0.54)");
                input.type = "text"
                input.name = dict[i].name || "";
                input.id = dict[i].name || "";
                input.readOnly = dict[i].readOnly;
                itemafter.appendChild(input);
                itemafter.className = "item-after";
                itemafter.setAttribute("style", "text-align:right;");
            }


            itemtitle.className = "item-title";
            if (dict[i].readOnly) {
                itemtitle.setAttribute("style", "color: #666666");
            }
            else {
                itemtitle.setAttribute("style", "color: #2b73af");
            }
            itemtitle.textContent = dict[i].display || "";

            //必填项
            if (dict[i].hasOwnProperty("validate") && dict[i].validate.required) {
                var $span = $('<span style="color:red">*</span>');
                $(itemtitle).append($span);
            }

            iteminner.appendChild(itemtitle);

            iteminner.className = "item-inner";
            if (iselect) {
                var itemContent = document.createElement('div');
                itemContent.className = "item-content";
                itemsmart.appendChild(select);

                itemContent.appendChild(iteminner);
                itemsmart.appendChild(itemContent);

                li.appendChild(itemsmart);

            }
            else {
                iteminner.appendChild(itemafter);
                iteminner.setAttribute("style", "padding: 0px;padding-left: 16px;padding-right:16px;");

                li.appendChild(iteminner)

            }


            ul.appendChild(li);

        }

        return ul.outerHTML;
    };
    //自动生成列表页
    app.CreateListContentHtml = function (data, dict, iCheckBox) {
        var ul = document.createElement('ul');
        for (var i = 0; i < data.length; i++) {
            var li = document.createElement('li');
            var aitem = document.createElement('a');
            var iteminner = document.createElement('div');
            var itemrowList = document.createElement('div');
            var itemcell = document.createElement('div');

            //复选框
            var checkLabel = document.createElement('label');
            var checkinput = document.createElement('input');
            var checki = document.createElement('i');
            checkinput.type = "checkbox";
            checki.className = "icon icon-checkbox";
            checkLabel.className = "item-checkbox item-checkbox-icon-start item-content";

            aitem.className = "item-link item-content";
            aitem.href = "#";
            iteminner.className = "item-inner item-cell";
            itemrowList.className = "item-row";
            itemcell.className = "item-cell";
            for (var j = 0; j < dict.length; j++) {
                if (dict[j].isPrimaryKey) {
                    itemrowList.setAttribute("id", `list-${data[i][dict[j].PrimaryKey]}`);
                    if (iCheckBox) {
                        checkinput.value = data[i][dict[j].PrimaryKey]
                    }
                    continue;
                }
                if (dict[j].isShow == false)
                    continue;
                var itemnogap = document.createElement('div');
                var itemcoltitle = document.createElement('div');
                var itemcoldata = document.createElement('div');
                var itemcol10 = document.createElement('div');
                itemnogap.className = "row no-gap";
                itemcoltitle.className = "col-33";
                itemcoldata.className = "col-65";
                itemcol10.className = "col-10";
                itemcoltitle.setAttribute("style", "color: rgb(192,192,192)");
                itemcoltitle.textContent = dict[j].display;
                if (dict[j].hasOwnProperty("Option") && dict[j].Option) {
                    let option = dict[j].Option.find(x => x.ID == data[i][(dict[j].name).toUpperCase()]);
                    itemcoldata.textContent = option ? option.TEXT : "";
                    let optionStyle = option ? option.style : "";
                    itemcoldata.setAttribute("style", optionStyle || "white-space: nowrap;overflow: hidden;text-overflow:ellipsis; padding-left: 30px;");
                }
                else {
                    itemcoldata.textContent = data[i][dict[j].name] || "";
                    itemcoldata.setAttribute("style", "white-space: nowrap;overflow: hidden;text-overflow:ellipsis; padding-left:30px;");

                }
                itemnogap.appendChild(itemcoltitle);
                itemnogap.appendChild(itemcoldata);
                itemnogap.appendChild(itemcol10);
                itemcell.appendChild(itemnogap);

                itemrowList.appendChild(itemcell);


                iteminner.appendChild(itemrowList)
                if (iCheckBox) {
                    checkLabel.appendChild(checkinput);
                    checkLabel.appendChild(checki);
                    checkLabel.appendChild(iteminner);
                    li.appendChild(checkLabel);
                }
                else {
                    li.appendChild(iteminner);
                }
                //aitem.appendChild(iteminner);

                //li.appendChild(aitem);
            }
            ul.appendChild(li);


        }
        return ul.outerHTML;
    };

    //导出
    window.app = app;
    window.show404 = function () {
        app.dialog.error('敬请期待');
    };
    var $viewMain = $$('.view-main');
    if (!$viewMain.hasClass("view-skip-init")) {
        app.methods.initMainView();
    }
})();