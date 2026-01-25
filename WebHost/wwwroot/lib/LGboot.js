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
    function getPath(js, doc) {
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

    var THEME_KEY = "preferences-theme", DARK_THEME = "eui/ligerui-dark.css", DARK_CLASS = "l-dark-mode";
    function isDarkTheme() {
        for (var i = 0, l = window.localStorage.length; i < l; i++) {
            var key = window.localStorage.key(i);
            if (key.endsWith(THEME_KEY)) {
                return /dark/.test(window.localStorage.getItem(key))
            }
        }
        return false;
    }
    var jsPath = getPath("LGboot.js", window.document);
    window.bootPATH = jsPath.path;
    window.JsVersion = jsVer || jsPath.ver || (new Date().toJSON()).substring(0, 10).replace(/-/g, "");

    //head
    function loadCSS(src, skipVersion) {
        src = bootPATH + src;
        if (skipVersion !== true) {
            src += "?ver=" + JsVersion;
        }
        var link = document.createElement('link');
        link.href = src;
        link.rel = 'stylesheet';
        link.type = 'text/css';
        document.head.appendChild(link);
    }

    function loadJS(src, skipVersion) {
        src = bootPATH + src;
        if (skipVersion !== true) {
            src += "?ver=" + JsVersion;
        }
        document.write('<script src="' + src + '" type="text/javascript"></script>');
    }

    loadCSS('dist/css/normalize.css', true);
    loadCSS('dist/css/font-awesome.css', true);
    loadJS('jquery/jquery.js', true);
    loadCSS('eui/ligerui.css');
    loadJS('eui/ligerui.js');
    loadCSS('css/ui.expand.css');
    loadJS('js/ui.expand.js');
    loadJS('js/common.js');
    loadJS('js/LG.js');
    loadJS('lc.js');

    if (isDarkTheme()) {
        setTimeout(function () {
            document.body.classList.add(DARK_CLASS);
        }, 300);
        loadCSS(DARK_THEME);
    }

    window.addEventListener('storage', function (event) {
        if (!event.key.endsWith(THEME_KEY)) {
            return;
        }
        var search = 'link[href*="' + DARK_THEME + '"]';
        document.body.classList.remove(DARK_CLASS);
        if (/dark/.test(event.newValue)) {
            if (!document.querySelector(search)) {
                document.body.classList.add(DARK_CLASS);
                loadCSS(DARK_THEME);
            }
        } else {
            var darkLink = document.querySelector(search);
            if (darkLink) {
                darkLink.parentNode.removeChild(darkLink);
            }
        }
    });
})();