; (function ($) {
    'use strict';
    !function (t, r) { "object" == typeof exports && "undefined" != typeof module ? module.exports = r(t) : "function" == typeof define && define.amd ? define(r) : r(t) }("undefined" != typeof self ? self : "undefined" != typeof window ? window : "undefined" != typeof global ? global : this, function (r) { "use strict"; var e = (r = r || {}).Base64, t = "", i = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/", n = function (t) { for (var r = {}, e = 0, n = t.length; e < n; e++)r[t.charAt(e)] = e; return r }(i), o = String.fromCharCode, a = function (t) { var r; if (t.length < 2) return (r = t.charCodeAt(0)) < 128 ? t : r < 2048 ? o(192 | r >>> 6) + o(128 | 63 & r) : o(224 | r >>> 12 & 15) + o(128 | r >>> 6 & 63) + o(128 | 63 & r); var r = 65536 + 1024 * (t.charCodeAt(0) - 55296) + (t.charCodeAt(1) - 56320); return o(240 | r >>> 18 & 7) + o(128 | r >>> 12 & 63) + o(128 | r >>> 6 & 63) + o(128 | 63 & r) }, u = /[\uD800-\uDBFF][\uDC00-\uDFFFF]|[^\x00-\x7F]/g, c = function (t) { return t.replace(u, a) }, f = function (t) { var r = [0, 2, 1][t.length % 3], t = t.charCodeAt(0) << 16 | (1 < t.length ? t.charCodeAt(1) : 0) << 8 | (2 < t.length ? t.charCodeAt(2) : 0), e; return [i.charAt(t >>> 18), i.charAt(t >>> 12 & 63), 2 <= r ? "=" : i.charAt(t >>> 6 & 63), 1 <= r ? "=" : i.charAt(63 & t)].join("") }, d = r.btoa && "function" == typeof r.btoa ? function (t) { return r.btoa(t) } : function (t) { if (t.match(/[^\x00-\xFF]/)) throw new RangeError("The string contains invalid characters."); return t.replace(/[\s\S]{1,3}/g, f) }, h = function (t) { return d(c(String(t))) }, s = function (t) { return t.replace(/[+\/]/g, function (t) { return "+" == t ? "-" : "_" }).replace(/=/g, "") }, l = function (t, r) { return r ? s(h(t)) : h(t) }, p = function (t) { return l(t, !0) }, g; r.Uint8Array && (g = function (t, r) { for (var e = "", n = 0, o = t.length; n < o; n += 3) { var a = t[n], u = t[n + 1], c = t[n + 2], a = a << 16 | u << 8 | c; e += i.charAt(a >>> 18) + i.charAt(a >>> 12 & 63) + (void 0 !== u ? i.charAt(a >>> 6 & 63) : "=") + (void 0 !== c ? i.charAt(63 & a) : "=") } return r ? s(e) : e }); var A = /[\xC0-\xDF][\x80-\xBF]|[\xE0-\xEF][\x80-\xBF]{2}|[\xF0-\xF7][\x80-\xBF]{3}/g, y = function (t) { switch (t.length) { case 4: var r, e = ((7 & t.charCodeAt(0)) << 18 | (63 & t.charCodeAt(1)) << 12 | (63 & t.charCodeAt(2)) << 6 | 63 & t.charCodeAt(3)) - 65536; return o(55296 + (e >>> 10)) + o(56320 + (1023 & e)); case 3: return o((15 & t.charCodeAt(0)) << 12 | (63 & t.charCodeAt(1)) << 6 | 63 & t.charCodeAt(2)); default: return o((31 & t.charCodeAt(0)) << 6 | 63 & t.charCodeAt(1)) } }, b = function (t) { return t.replace(A, y) }, x = function (t) { var r = t.length, e = r % 4, t = (0 < r ? n[t.charAt(0)] << 18 : 0) | (1 < r ? n[t.charAt(1)] << 12 : 0) | (2 < r ? n[t.charAt(2)] << 6 : 0) | (3 < r ? n[t.charAt(3)] : 0), t = [o(t >>> 16), o(t >>> 8 & 255), o(255 & t)]; return t.length -= [0, 0, 2, 1][e], t.join("") }, B = r.atob && "function" == typeof r.atob ? function (t) { return r.atob(t) } : function (t) { return t.replace(/\S{1,4}/g, x) }, C = function (t) { return B(String(t).replace(/[^A-Za-z0-9\+\/]/g, "")) }, v = function (t) { return b(B(t)) }, F = function (t) { return String(t).replace(/[-_]/g, function (t) { return "-" == t ? "+" : "/" }).replace(/[^A-Za-z0-9\+\/]/g, "") }, m = function (t) { return v(F(t)) }, S; r.Uint8Array && (S = function (t) { return Uint8Array.from(C(F(t)), function (t) { return t.charCodeAt(0) }) }); var w = function () { var t = r.Base64; return r.Base64 = e, t }, j; return r.encryptFront = function (t, r) { var e = (t = l(t)).length, n, o; return 1 < e && (n = Math.floor(Math.random() * (e - 1)), o = parseInt(e / 2), t = t.substring(o, e) + t.substr(n, 1) + t.substring(0, o)), t }, r.decryptFront = function (t, r) { try { var e = (t.length + 1) / 2; return 0 < e && (t = t.substr(e) + t.substring(0, e - 1)), m(t) } catch (n) { return t } }, r.Base64 = { VERSION: t, atob: C, btoa: d, fromBase64: m, toBase64: l, utob: c, encode: l, encodeURI: p, btou: b, decode: m, noConflict: w, fromUint8Array: g, toUint8Array: S }, "function" == typeof Object.defineProperty && (j = function (t) { return { value: t, enumerable: !1, writable: !0, configurable: !0 } }, r.Base64.extendString = function () { Object.defineProperty(String.prototype, "fromBase64", j(function () { return m(this) })), Object.defineProperty(String.prototype, "toBase64", j(function (t) { return l(this, t) })), Object.defineProperty(String.prototype, "toBase64URI", j(function () { return l(this, !0) })) }), r.Meteor && (Base64 = r.Base64), "undefined" != typeof module && module.exports ? module.exports.Base64 = r.Base64 : "function" == typeof define && define.amd && define([], function () { return r.Base64 }), { Base64: r.Base64 } });

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
            url = url.replace(/\.\.\//g, "");
            baseUrl = baseUrl || this.apiBase;
            if (baseUrl && url.indexOf(baseUrl) === 0) return url;
            url = baseUrl + url;
            return url;
        }
    };

    $.extend(window, {//全局方法扩展
        encrypt: window.encryptFront,
        getQueryString: function () {
            var result = location.search.match(new RegExp("[\?\&][^\?\&]+=[^\?\&]+", "g"));
            if (result == null) {
                return "";
            }
            for (var i = 0; i < result.length; i++) {
                result[i] = result[i].substring(1);
            }
            return result;
        },
        getQueryStringByName: function (name) {
            var result = location.search.match(new RegExp("[\?\&]" + name + "=([^\&]+)", "i"));
            if (result == null || result.length < 1) {
                return "";
            }
            return decodeURIComponent(result[1]);
        },
        generateJsToken: function (jqXHR, opt) {//js票据 eval用到jqXHR
            opt = opt || { jsToken: "JsToken" };
            var key = opt.jsToken;
            if (key === true) {
                key = gksybConfigs.getUrl(opt.url || "").replace(/^\/|(\?.*)$/g, '').replace(/\/$/, "");
            }
            var tokenOptions = $.extend(true, {
                noGlobal: true,
                url: "Auth/JsToken",
                async: false,
                data: { key: key },
                dataType: "text",
                type: 'post',
                success: function (result) {
                    eval(result);
                },
                error: function () { }
            }, opt.tokenOptions);
            $.ajax(tokenOptions);
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
            return;
        },
        refreshGksybToken: function (callback, _toLogin) {
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
    var arrayRemoveNull = function () {
        if (Object.prototype.toString.apply(this) !== "[object Array]") return null;
        for (var i = this.length - 1; i >= 0; i--) {
            var item = this[i];
            if (item !== 0 && !this[i]) this.splice(i, 1);
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
                innerTip("登录信息已失效，请重新登录。", option._toLogin);
                return;
            }
            option.redo = true;
            window.refreshGksybToken(function () {
                $.ajax(option); //登陆后重发请求
            }, option._toLogin);
        } else {
            if (!opt.error) {
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
    if (!window.session) initStorage("session", "GksybData");
    if (!window.tempStorage) initStorage("tempStorage", "GksybTemp");
    if (!window.ticket) initStorageString("ticket", "GksybTicket");

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
})(jQuery);