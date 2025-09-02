; (function ($) {
    'use strict';
    !(function (n, t) { typeof exports == "object" && typeof module != "undefined" ? module.exports = t(n) : typeof define == "function" && define.amd ? define(t) : t(n) })(typeof self != "undefined" ? self : typeof window != "undefined" ? window : typeof global != "undefined" ? global : this, function (n) { "use strict"; var b, e; n = n || {}; var k = n.Base64, i = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/", u = function (n) { for (var i = {}, t = 0, r = n.length; t < r; t++)i[n.charAt(t)] = t; return i }(i), t = String.fromCharCode, d = function (n) { var i; return n.length < 2 ? (i = n.charCodeAt(0), i < 128 ? n : i < 2048 ? t(192 | i >>> 6) + t(128 | i & 63) : t(224 | i >>> 12 & 15) + t(128 | i >>> 6 & 63) + t(128 | i & 63)) : (i = 65536 + (n.charCodeAt(0) - 55296) * 1024 + (n.charCodeAt(1) - 56320), t(240 | i >>> 18 & 7) + t(128 | i >>> 12 & 63) + t(128 | i >>> 6 & 63) + t(128 | i & 63)) }, g = /[\uD800-\uDBFF][\uDC00-\uDFFFF]|[^\x00-\x7F]/g, o = function (n) { return n.replace(g, d) }, nt = function (n) { var r = [0, 2, 1][n.length % 3], t = n.charCodeAt(0) << 16 | (n.length > 1 ? n.charCodeAt(1) : 0) << 8 | (n.length > 2 ? n.charCodeAt(2) : 0), u = [i.charAt(t >>> 18), i.charAt(t >>> 12 & 63), r >= 2 ? "=" : i.charAt(t >>> 6 & 63), r >= 1 ? "=" : i.charAt(t & 63)]; return u.join("") }, s = n.btoa && typeof n.btoa == "function" ? function (t) { return n.btoa(t) } : function (n) { if (n.match(/[^\x00-\xFF]/)) throw new RangeError("The string contains invalid characters."); return n.replace(/[\s\S]{1,3}/g, nt) }, h = function (n) { return s(o(String(n))) }, c = function (n) { return n.replace(/[+\/]/g, function (n) { return n == "+" ? "-" : "_" }).replace(/=/g, "") }, r = function (n, t) { return t ? c(h(n)) : h(n) }, tt = function (n) { return r(n, !0) }, l; n.Uint8Array && (l = function (n, t) { for (var f = "", r = 0, s = n.length; r < s; r += 3) { var h = n[r], e = n[r + 1], o = n[r + 2], u = h << 16 | e << 8 | o; f += i.charAt(u >>> 18) + i.charAt(u >>> 12 & 63) + (typeof e != "undefined" ? i.charAt(u >>> 6 & 63) : "=") + (typeof o != "undefined" ? i.charAt(u & 63) : "=") } return t ? c(f) : f }); var it = /[\xC0-\xDF][\x80-\xBF]|[\xE0-\xEF][\x80-\xBF]{2}|[\xF0-\xF7][\x80-\xBF]{3}/g, rt = function (n) { switch (n.length) { case 4: var r = (7 & n.charCodeAt(0)) << 18 | (63 & n.charCodeAt(1)) << 12 | (63 & n.charCodeAt(2)) << 6 | 63 & n.charCodeAt(3), i = r - 65536; return t((i >>> 10) + 55296) + t((i & 1023) + 56320); case 3: return t((15 & n.charCodeAt(0)) << 12 | (63 & n.charCodeAt(1)) << 6 | 63 & n.charCodeAt(2)); default: return t((31 & n.charCodeAt(0)) << 6 | 63 & n.charCodeAt(1)) } }, a = function (n) { return n.replace(it, rt) }, ut = function (n) { var i = n.length, e = i % 4, r = (i > 0 ? u[n.charAt(0)] << 18 : 0) | (i > 1 ? u[n.charAt(1)] << 12 : 0) | (i > 2 ? u[n.charAt(2)] << 6 : 0) | (i > 3 ? u[n.charAt(3)] : 0), f = [t(r >>> 16), t(r >>> 8 & 255), t(r & 255)]; return f.length -= [0, 0, 2, 1][e], f.join("") }, v = n.atob && typeof n.atob == "function" ? function (t) { return n.atob(t) } : function (n) { return n.replace(/\S{1,4}/g, ut) }, y = function (n) { return v(String(n).replace(/[^A-Za-z0-9\+\/]/g, "")) }, ft = function (n) { return a(v(n)) }, p = function (n) { return String(n).replace(/[-_]/g, function (n) { return n == "-" ? "+" : "/" }).replace(/[^A-Za-z0-9\+\/]/g, "") }, f = function (n) { return ft(p(n)) }, w; return n.Uint8Array && (w = function (n) { return Uint8Array.from(y(p(n)), function (n) { return n.charCodeAt(0) }) }), b = function () { var t = n.Base64; return n.Base64 = k, t }, n.encryptFront = function (n) { var t, u, i; return n = r(n), t = n.length, t > 1 && (u = Math.floor(Math.random() * (t - 1)), i = parseInt(t / 2), n = n.substring(i, t) + n.substr(u, 1) + n.substring(0, i), n = "pqz" + n.split("").reverse().join("") + "zpq"), n }, n.decryptFront = function (n) { try { n = n.length > 5 ? n.substr(3, n.length - 6) : n; n = n.split("").reverse().join(""); var t = (n.length + 1) / 2; return t > 0 && (n = n.substr(t) + n.substring(0, t - 1)), f(n) } catch (i) { return n } }, n.Base64 = { VERSION: "", atob: y, btoa: s, fromBase64: f, toBase64: r, utob: o, encode: r, encodeURI: tt, btou: a, decode: f, noConflict: b, fromUint8Array: l, toUint8Array: w }, typeof Object.defineProperty == "function" && (e = function (n) { return { value: n, enumerable: !1, writable: !0, configurable: !0 } }, n.Base64.extendString = function () { Object.defineProperty(String.prototype, "fromBase64", e(function () { return f(this) })); Object.defineProperty(String.prototype, "toBase64", e(function (n) { return r(this, n) })); Object.defineProperty(String.prototype, "toBase64URI", e(function () { return r(this, !0) })) }), typeof module != "undefined" && module.exports ? module.exports.Base64 = n.Base64 : typeof define == "function" && define.amd && define([], function () { return n.Base64 }), { Base64: n.Base64 } });

    var urlBase = "/";
    for (var i = 0, l = document.scripts.length; i < l; i++) {
        var src = document.scripts[i].src;
        var index = src.indexOf("lib/js/common.js");
        if (index > 0) {
            urlBase = src.substring(0, index);
            var host = "//" + location.host;
            index = urlBase.indexOf(host);
            if (index > 0) urlBase = urlBase.substring((index + host.length));
            break;
        }
    }
    window.gksybConfigs = {//全局配置
        urlBase: urlBase, // url访问路径
        apiBase: urlBase, // api 接口的访问路径
        getUrl: function (url, baseUrl) {
            if (url.toLowerCase().indexOf("http") === 0) return url;
            url = url.replace(/\.\.\//g, "").replace(/\/+/g, "\/");
            baseUrl = baseUrl || this.apiBase;
            if (baseUrl && url.indexOf(baseUrl) === 0) return url;
            url = baseUrl + url;
            return url;
        }
    };

    $.extend(window, {//全局方法扩展
        encrypt: window.encryptFront,
        getQueryString: function (url) {
            url = url || location.search;
            var result = url.match(new RegExp("[\?\&][^\?\&]+=[^\?\&]+", "g"));
            if (result == null) {
                return "";
            }
            for (var i = 0; i < result.length; i++) {
                result[i] = result[i].substring(1);
            }
            return result;
        },
        getQueryStringByName: function (name, url) {
            url = url || location.search;
            var result = url.match(new RegExp("[\?\&]" + name + "=([^\&]+)", "i"));
            if (result == null || result.length < 1) {
                return "";
            }
            return decodeURIComponent(result[1]);
        },
        generateJsToken: function (jqXHR, opt) {//js票据 eval用到jqXHR
            opt = opt || { jsToken: "JsToken" };
            var key = opt.jsToken;
            if (key === true) {
                key = gksybConfigs.getUrl(opt.url || "").replace(gksybConfigs.apiBase, "").replace(/^\/|(\?.*)$/g, '').replace(/\/$/, "");
            }
            var data = (typeof key === "string" ? { key: key } : null);
            var tokenOptions = $.extend(true, {
                noGlobalBeforeSend: true,
                url: "Auth/JsToken",
                async: false,
                data: data,
                dataType: "text",
                type: 'post',
                _toLogin: opt._toLogin,
                success: function (result) {
                    eval(result);
                },
                error: function () { }
            }, opt.tokenOptions || (data === null ? key : null));
            $.ajax(tokenOptions);
        },
        setGksybToken: function (jqXHR) {//token验证
            if (window.session.Token) jqXHR.setRequestHeader("GKSYBTOKEN", window.session.Token);
        },
        _toLogin: function () {
            var loc = topDomainWindow.location;
            var loginUrl = gksybConfigs.urlBase + "login.html?FromUrl=" + encodeURIComponent(loc.href);
            loc.href = loginUrl;
        },
        innerDialogTip: function (msg, _toLogin) {
            if (_toLogin === true) {
                window._toLogin();
                return;
            }
            if ($.ligerDialog) {
                $.ligerDialog.error(msg, '提示信息', function () {
                    window._toLogin();
                }, {
                    allowClose: false
                });
            } else {
                alert(msg);
                window._toLogin();
            }
        },
        refreshGksybToken: function (callback, _toLogin, error) {
            var innerTip = window.innerDialogTip;
            $.ajax({
                async: false,
                noGlobal: false,
                type: 'post',
                dataType: 'json',
                url: 'Auth/RefreshToken',
                beforeSend: function (jqXHR) {
                    jqXHR.setRequestHeader("ticket", window.ticket);
                },
                success: function (result) {
                    if (result && !result.IsError) {
                        window.ticket = result.Data.Ticket;
                        delete result.Data.Ticket;
                        window.session = result.Data;
                        callback();
                        return;
                    }
                    if (error) {
                        error(result);
                        return;
                    }
                    innerTip("登录信息已失效，请重新登录。", _toLogin);
                },
                error: function (jqXHR, opt, thrownError) {
                    if (error) {
                        error(thrownError);
                        return;
                    }
                    innerTip("登录信息已失效，请重新登录。", _toLogin);
                }
            });
        }
    });

    //字符串去两边空格
    var stringTrim = function () {
        if (Object.prototype.toString.apply(this) !== "[object String]") return null;
        if ($.trim) return $.trim(this);
        return this.replace(/^[\s\uFEFF\xA0]+|[\s\uFEFF\xA0]+$/g, '');
    };
    try {
        String.prototype.trim || (Object.defineProperty && Object.defineProperty(String.prototype, "trim", {
            configurable: true,
            value: stringTrim
        }));
    } catch (e) { };
    String.prototype.trim || (String.prototype.trim = stringTrim);

    //字符串转日期
    var stringToDate = function (format) {
        if (Object.prototype.toString.apply(this) !== "[object String]") return null;
        if (/^\/Date/.test(this)) { // /Date(1328423451489)/
            var value = this.replace(/^\//, "new ").replace(/\/$/, "");
            eval("value = " + value);
            if (!value) return null;
            return value;
        }

        function _getMatch(format) {
            var r = [-1, -1, -1, -1, -1, -1],
                groupIndex = 0,
                regStr = "^",
                str = (format || "yyyy-MM-dd hh:mm:ss").replace(/H/g, "h");
            while (true) {
                var tmp_r = str.match(/^yyyy|MM|dd|mm|hh|HH|ss|-|\/|:|\s/);
                if (tmp_r) {
                    var c = tmp_r[0].charAt(0);
                    var mathLength = tmp_r[0].length;
                    var index = 'yMdhms'.indexOf(c);
                    if (index != -1) {
                        r[index] = groupIndex + 1;
                        regStr += "(\\d{1," + mathLength + "})";
                    } else {
                        var st = c == ' ' ? '\\s' : c;
                        regStr += "(" + st + ")";
                    }
                    groupIndex++;
                    if (mathLength == str.length) {
                        regStr += "$";
                        break;
                    }
                    str = str.substring(mathLength);
                } else {
                    return null;
                }
            }
            return {
                reg: new RegExp(regStr),
                position: r
            };
        }
        var r = _getMatch(format);
        if (!r) return null;
        var t = this.match(r.reg);
        if (!t) return null;
        var tt = {
            y: r.position[0] == -1 ? 1900 : t[r.position[0]],
            M: r.position[1] == -1 ? 0 : parseInt(t[r.position[1]], 10) - 1,
            d: r.position[2] == -1 ? 1 : parseInt(t[r.position[2]], 10),
            h: r.position[3] == -1 ? 0 : parseInt(t[r.position[3]], 10),
            m: r.position[4] == -1 ? 0 : parseInt(t[r.position[4]], 10),
            s: r.position[5] == -1 ? 0 : parseInt(t[r.position[5]], 10)
        };
        if (tt.M < 0 || tt.M > 11 || tt.d < 0 || tt.d > 31) return null;
        if (tt.m < 0 || tt.m > 59 || tt.h < 0 || tt.h > 23 || tt.s < 0 || tt.s > 59) return null;
        var d = new Date(tt.y, tt.M, tt.d, tt.h, tt.m, tt.s);
        return d;
    };
    try {
        String.prototype.toDate || (Object.defineProperty && Object.defineProperty(String.prototype, "toDate", {
            configurable: true,
            value: stringToDate
        }));
    } catch (e) { };
    String.prototype.toDate || (String.prototype.toDate = stringToDate);

    //日期格式化
    var dateFormat = function (format) {
        if (Object.prototype.toString.apply(this) !== "[object Date]") return null;
        format = format || "yyyy-MM-dd hh:mm:ss";
        var weekday = ["日", "一", "二", "三", "四", "五", "六"];
        var o = {
            "M+": this.getMonth() + 1, //month
            "d+": this.getDate(), //day
            "H+": this.getHours(), //hour
            "h+": this.getHours(), //hour
            "m+": this.getMinutes(), //minute
            "s+": this.getSeconds(), //second
            "q+": Math.floor((this.getMonth() + 3) / 3), //quarter
            "f+": this.getMilliseconds(), //millisecond
            "W": weekday[this.getDay()] //millisecond
        }

        if (/(y+)/.test(format)) {
            format = format.replace(RegExp.$1, (this.getFullYear() + "").substring(4 - RegExp.$1.length));
        }

        for (var k in o) {
            if (new RegExp("(" + k + ")").test(format)) {
                format = format.replace(RegExp.$1, RegExp.$1.length == 1 ? o[k] : ("00" + o[k]).substring(("" + o[k]).length));
            }
        }
        return format;
    };
    try {
        Date.prototype.format || (Object.defineProperty && Object.defineProperty(Date.prototype, "format", {
            configurable: true,
            value: dateFormat
        }));
    } catch (e) { };
    Date.prototype.format || (Date.prototype.format = dateFormat);

    //去除数组空元素
    var arrayRemoveNull = function (fn) {
        if (Object.prototype.toString.apply(this) !== "[object Array]") return null;
        fn = fn || function (item) {
            return item !== 0 && !item;
        };
        for (var i = this.length - 1; i >= 0; i--) {
            var item = this[i];
            if (fn(item)) this.splice(i, 1);
        }
        return this;
    };
    try {
        Array.prototype.removeNull || (Object.defineProperty && Object.defineProperty(Array.prototype, "removeNull", {
            configurable: true,
            value: arrayRemoveNull
        }));
    } catch (e) { };
    Array.prototype.removeNull || (Array.prototype.removeNull = arrayRemoveNull);

    //数组转树形
    var arrayToTree = function (id, pid, childrenName) {
        if (Object.prototype.toString.apply(this) !== "[object Array]") return;
        if (!this || !this.length) return [];
        childrenName = childrenName || "children";
        var data = this;
        var targetData = [];                    //存储数据的容器(返回)
        var records = {};
        var itemLength = data.length;           //数据集合的个数
        for (var i = 0; i < itemLength; i++) {
            var o = data[i];
            delete o[childrenName];
            var key = getKey(o[id]);
            if (key === null || key === undefined) continue;
            records[key] = o;
        }
        for (var i = 0; i < itemLength; i++) {
            var currentData = data[i];
            var key = getKey(currentData[pid]);
            var parentData = records[key];
            if (!parentData) {
                targetData.push(currentData);
                continue;
            }
            parentData[childrenName] = parentData[childrenName] || [];
            parentData[childrenName].push(currentData);
        }
        return targetData;

        function getKey(key) {
            if (key === 0) return "0";
            return (key || "").toString();
        }
    };
    try {
        Array.prototype.toTree || (Object.defineProperty && Object.defineProperty(Array.prototype, "toTree", {
            configurable: true,
            value: arrayToTree
        }));
    } catch (e) { };
    Array.prototype.toTree || (Array.prototype.toTree = arrayToTree);

    $.ajaxSetup({
        processData: false,
        paramData: true
    });

    $(document).unbind("ajaxError").ajaxError(function (event, jqXHR, opt, thrownError) {
        if (opt.noGlobal) return; //不触发全局函数
        if (opt.noGlobalError) return;
        if (jqXHR.status === 999) {
            var option = (jqXHR.orginOpt || opt);
            var innerTip = window.innerDialogTip;
            if ($.ligerDialog) {
                $.ligerDialog.hide();
                $.ligerDialog.close();
            }
            if (option.redo === true) {
                if (opt.errorOther) {
                    opt.errorOther(jqXHR, thrownError);
                    return;
                }
                innerTip("登录信息已失效，请重新登录。", option._toLogin);
                return;
            }
            option.redo = true;
            window.refreshGksybToken(function () {
                $.ajax(option); //登陆后重发请求
            }, option._toLogin, (opt.errorOther ? function () {
                opt.errorOther(jqXHR, thrownError);
            } : undefined));
        } else {
            if (!opt.error) {
                if (opt.errorOther) {
                    opt.errorOther(jqXHR, thrownError);
                    return;
                }
                var errorMsg = '请求数据出错,页面即将跳转!<br/>原因为：' + (jqXHR.responseText.substring(0, 50) || "") + "<br/>错误码:" + (jqXHR.status || "") + (thrownError || "");
                if ($.ligerDialog) {
                    $.ligerDialog.hide();
                    $.ligerDialog.close();
                    $.ligerDialog.error(errorMsg, "操作失败", function () {
                        //location.reload();
                    });
                } else {
                    alert(errorMsg);
                }
            }
        }
    }).unbind("ajaxSend").ajaxSend(function (e, jqXHR, opt) {
        if (opt.jsToken) { //防止机器人提交
            window.generateJsToken(jqXHR, opt);
            delete opt.tokenOptions;
        }
        if (!opt.noGlobal) {
            delete jqXHR.orginOpt;
            jqXHR.orginOpt = $.extend(true, {}, opt);
        }
        if (opt.skipUrlHandle !== true) opt.url = gksybConfigs.getUrl(opt.url || "");
        window.setGksybToken(jqXHR);
        var contentType = (opt.contentType || "").toLocaleLowerCase();
        var isJson = contentType.indexOf("application/json") >= 0;
        var dataType = Object.prototype.toString.apply(opt.data);
        if (!(opt.noGlobal || opt.noGlobalBeforeSend)) {
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
                if (!opt.data) {
                    jqXHR.setRequestHeader("Content-Type", opt.contentType);
                }
            }
        }
        if (isJson && dataType !== "[object String]") {
            opt.data = JSON.stringify(opt.data);
        }
        else if (opt.data && opt.paramData && dataType !== "[object String]") {
            opt.data = $.param(opt.data, opt.traditional);
        }
    });

    var initStorage = function (name, key) {
        Object.defineProperty(window, name, {
            get: function () {
                var data = window.localStorage.getItem(key);
                if (data) return JSON.parse(data);
                return data || {};
            },
            set: function (val) {
                if (val && typeof val !== "string") {
                    val = JSON.stringify(val);
                }
                if (val) {
                    window.localStorage.setItem(key, val);
                } else {
                    window.localStorage.removeItem(key);
                }
            }
        });
    }
    var initStorageString = function (name, key) {
        Object.defineProperty(window, name, {
            get: function () {
                return window.localStorage.getItem(key) || "";
            },
            set: function (val) {
                if (val) {
                    window.localStorage.setItem(key, val);
                } else {
                    window.localStorage.removeItem(key);
                }
            }
        });
    }
    if (window.session === undefined) initStorage("session", "GksybData");
    if (window.tempStorage === undefined) initStorage("tempStorage", "GksybTemp");
    if (window.ticket === undefined) initStorageString("ticket", "GksybTicket");
    if (window[imeiKey] === undefined) {
        var imeiKey = "GksybIMEI";
        Object.defineProperty(window, imeiKey, {
            get: function () {
                var val = window.localStorage.getItem(imeiKey);
                if (val) return val;
                val = new Date().getTime();
                $.ajax({
                    noGlobalBeforeSend: true,
                    url: 'auth/imei',
                    async: false,
                    dataType: 'json',
                    type: 'post',
                    success: function (result) {
                        if (!result || result.IsError) return;
                        val = result.Data;
                    }
                });
                window.localStorage.setItem(imeiKey, val);
                return val;
            }
        });
    }

    if (window.topWindow === undefined) {
        Object.defineProperty(window, 'topWindow', {
            get: function () {//获取不跨域的有gksybConfigs的顶层窗口
                var parentWindow = window;
                try {
                    for (var i = 0; i < 10; i++) {
                        if (parentWindow.parent.location.href && parentWindow.parent.setGksybToken) {
                            parentWindow = parentWindow.parent;
                        }
                    }
                } catch (err) {
                }
                return parentWindow;
            }
        });
    }

    if (window.topDomainWindow === undefined) {
        Object.defineProperty(window, 'topDomainWindow', {
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
})(jQuery);