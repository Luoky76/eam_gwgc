; (function () {
    var jsVer = window.localStorage.getItem("_jsVersion");
    if (!jsVer) {
        var parentWindow = window;
        try {
            for (var i = 0; i < 10; i++) {
                if (parentWindow.parent.location.href && parentWindow.parent.JsVersion) {
                    parentWindow = parentWindow.parent;
                    jsVer = parentWindow.JsVersion;
                }
            }
        } catch (err) {
        }
    }
    function GetPath(js, doc) {
        var path = "/", ver = "";
        for (var i = 0, l = doc.scripts.length; i < l; i++) {
            var src = doc.scripts[i].src;
            var index = src.indexOf(js);
            if (index > 0) {
                ver = src.split("ver=")[1];
                path = src.substring(0, index);
                var host = "//" + location.host;
                index = path.indexOf(host);
                if (index > 0) path = path.substring((index + host.length));
                break;
            }
        }
        return { path: path, ver: ver };
    }
    var jsPath = GetPath("LGboot.js", window.document);
    window.bootPATH = jsPath.path;
    window.JsVersion = jsVer || jsPath.ver || (new Date().toJSON()).substring(0, 10).replace(/-/g, "");

    //head
    document.write('<link href="' + bootPATH + 'dist/css/normalize.css" rel="stylesheet" type="text/css" />');
    document.write('<link href="' + bootPATH + 'dist/css/font-awesome.css" rel="stylesheet" type="text/css" />');
    document.write('<script src="' + bootPATH + 'jquery/jquery.js" type="text/javascript"></sc' + 'ript>');
    document.write('<link href="' + bootPATH + 'eui/ligerui.css?ver=' + JsVersion + '" rel="stylesheet" type="text/css" />');
    document.write('<script src="' + bootPATH + 'eui/ligerui.js?ver=' + JsVersion + '" type="text/javascript"></sc' + 'ript>');
    document.write('<link href="' + bootPATH + 'css/ui.expand.css?ver=' + JsVersion + '" rel="stylesheet" type="text/css" />');
    document.write('<script src="' + bootPATH + 'js/ui.expand.js?ver=' + JsVersion + '" type="text/javascript"></sc' + 'ript>');
    document.write('<script src="' + bootPATH + 'js/common.js?ver=' + JsVersion + '" type="text/javascript"></sc' + 'ript>');
    document.write('<script src="' + bootPATH + 'js/LG.js?ver=' + JsVersion + '" type="text/javascript"></sc' + 'ript>');
    document.write('<script src="' + bootPATH + 'lc.js?ver=' + JsVersion + '" type="text/javascript"></sc' + 'ript>');
})();