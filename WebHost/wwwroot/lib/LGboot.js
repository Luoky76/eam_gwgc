__CreateJSPath = function (js, mydocument) {
    var scripts = mydocument.getElementsByTagName("script");
    var path = "";
    var jsVersion = "";
    for (var i = 0, l = scripts.length; i < l; i++) {
        var src = scripts[i].src;
        if (src.indexOf(js) != -1) {
            var ss = src.split(js);
            path = ss[0];
            jsVersion = src.split("ver=")[1];
            break;
        }
    }
    if (!path) return null;
    if (path.indexOf("https:") == -1 && path.indexOf("http:") == -1 && path.indexOf("file:") == -1 && path.indexOf("\/") != 0) {
        var href = location.href;
        href = href.split("#")[0];
        href = href.split("?")[0];
        var ss = href.split("/");
        ss.length = ss.length - 1;
        href = ss.join("/");
        path = href + "/" + path;
    }
    return {
        path: path,
        jsVersion: jsVersion
    };
}
var parentWindow = window;
var url = parentWindow.location.href || "";
try {
    for (var i = 0; i < 10; i++) {
        if (parentWindow.parent) {
            url = parentWindow.parent.location.href || "";
            parentWindow = parentWindow.parent;
        }
    }
} catch (err) {
}
var jsPath = __CreateJSPath("LGboot.js", parentWindow.document) || __CreateJSPath("LGboot.js", window.document);
var JsVersion = window.localStorage.getItem("_jsVersion") || jsPath.jsVersion || (new Date().toJSON()).substring(0, 10).replace(/-/g, "");
var bootPATH = jsPath.path;
//head
document.write('<link href="' + bootPATH + 'dist/css/normalize.css" rel="stylesheet" type="text/css" />');
document.write('<link href="' + bootPATH + 'dist/css/font-awesome.css" rel="stylesheet" type="text/css" />');
document.write('<script src="' + bootPATH + 'jquery/jquery.js" type="text/javascript"></sc' + 'ript>');
document.write('<link href="' + bootPATH + 'eui/ligerui.css?ver=' + JsVersion + '" rel="stylesheet" type="text/css" />');
document.write('<script src="' + bootPATH + 'eui/ligerui.js?ver=' + JsVersion + '" type="text/javascript"></sc' + 'ript>');
document.write('<link href="' + bootPATH + 'css/ui.expand.css?ver=1.4" rel="stylesheet" type="text/css" />');
document.write('<script src="' + bootPATH + 'js/ui.expand.js?ver=' + JsVersion + '" type="text/javascript"></sc' + 'ript>');
document.write('<script src="' + bootPATH + 'js/common.js?ver=' + JsVersion + '" type="text/javascript"></sc' + 'ript>');
document.write('<script src="' + bootPATH + 'js/LG.js?ver=' + JsVersion + '" type="text/javascript"></sc' + 'ript>');
document.write('<script src="' + bootPATH + 'lc.js?ver=' + JsVersion + '" type="text/javascript"></sc' + 'ript>');