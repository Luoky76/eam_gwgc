if (window.gksybConfigs && window.gksybConfigs.getUrl) {
    window.UEDITOR_HOME_URL = window.gksybConfigs.getUrl("/lib/neditor/", window.gksybConfigs.urlBase);
}
document.write('<script src="' + bootPATH + 'neditor/neditor.config.js?ver=' + JsVersion + '"  type="text/javascript" charset="utf-8" ></sc' + 'ript>');
document.write('<script src="' + bootPATH + 'neditor/neditor.min.js" type="text/javascript" charset="utf-8" ></sc' + 'ript>');
document.write('<script src="' + bootPATH + 'neditor/i18n/zh-cn/zh-cn.js" type="text/javascript"  charset="utf-8" ></sc' + 'ript>');
document.write('<script src="' + bootPATH + 'neditor/neditor.service.js?ver=' + JsVersion + '"  type="text/javascript" charset="utf-8" ></sc' + 'ript>');