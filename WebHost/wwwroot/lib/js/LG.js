(function ($) {
    'use strict';
    $.extend($.ligerDefaults.ComboBox, {
        selectBoxHeight: 300
    });

    $.extend($.ligerDefaults.Grid, {
        toolbarShowInLeft: true
    });

    $.extend($.ligerui.controls.Grid.prototype, {
        getChangedRows: function (trim) {
            var g = this,
                changedRows = new Object();
            if (trim === undefined) {
                trim = false;
            }
            var added = g.getAdded();
            var updated = g.getUpdated();
            var deleted = g.getDeleted();
            var original = [];
            for (var i = 0, l = updated.length; i < l; i++) {
                var rowdata = updated[i];
                original.push(rowdata.__original);
                delete rowdata.__original;
            }
            if (trim) {
                if (added) {
                    $.trimAll(added);
                }
                if (updated) {
                    $.trimAll(updated);
                }
            }
            changedRows["added"] = added;
            changedRows["updated"] = updated;
            changedRows["original"] = original;
            changedRows["deleted"] = deleted;
            return changedRows;
        }
    });

    //全局系统对象
    window['LG'] = {};
    //右下角的提示框
    LG.tip = function (message, options) {
        if (typeof message !== 'string') {
            options = $.extend(message, options);
            message = null;
        }
        $.ligerDialog.tip($.extend({
            content: message
        }, options));
    };

    //显示loading
    LG.showLoading = function (message, options) {
        $.ligerDialog.waitting(message || "正在加载中...", options);
    };
    //隐藏loading
    LG.hideLoading = function () {
        $.ligerDialog.closeWaitting();
    }
    //显示成功提示窗口
    LG.showSuccess = function (message, callback, options) {
        if (typeof (message) == "function" || arguments.length == 0) {
            callback = message;
            message = "操作成功!";
        }
        message = message.replace(/\r\n/g, "<br>").replace(/\n/g, "<br>").replace(/\s/g, "&nbsp;");
        $.ligerDialog.success(message, '提示信息', callback, options || {
            allowClose: false
        });
    };
    //显示失败提示窗口
    LG.showError = function (message, callback, options) {
        if (typeof (message) == "function" || arguments.length == 0) {
            callback = message;
            message = "操作失败!";
        }
        message = message.replace(/\r\n/g, "<br>").replace(/\n/g, "<br>").replace(/\s/g, "&nbsp;");
        $.ligerDialog.error(message, '提示信息', callback, options || {
            allowClose: false
        });
    };

    //提交服务器请求
    //返回json格式
    //1,提交给类 options.type  方法 options.method 处理
    //2,并返回 AjaxResult(这也是一个类)类型的的序列化好的字符串
    LG.ajax = function (options) {
        options.url = options.url || options.ashxUrl;
        options.contentType = options.contentType || "application/json";
        options.successInner = options.success;
        options.errorInner = options.error;
        options.type = options.type || 'post';

        delete options.ashxUrl;
        delete options.success;
        delete options.error;
        var p = $.extend(true, {
            dataType: 'json',
            type: 'post',
            beforeSend: function (jqXHR, opt) {
                LG.showLoading(p.loading, { maskClose: false });
            },
            complete: function () {
                LG.hideLoading();
            },
            success: function (result) {
                if (!result) return;
                if (result.IsError) {
                    if (options.errorInner) options.errorInner(result.Message, result.Data);
                } else {
                    if (options.successInner) options.successInner(result.Data, result.Message);
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                LG.showError('请求数据出错,页面即将跳转!<br/>原因为：' + (XMLHttpRequest.responseText || "") + "<br/>错误码:" + (XMLHttpRequest.status || "") + (errorThrown || ""),
                    function () {
                        location.reload();
                    });
            }
        }, options);
        $.ajax(p);
    };

    LG.ComboBoxAjax = function (options) {
        options.url = options.url || "common/jsonValueMul";
        options.async = options.async || false;
        LG.ajax(options);
    };

    //ajax验证数据
    LG.ValidAjax = function (input, value, p) {
        options.async = options.async || false;
        options.success = function (Data, Message) {
            if (!Data) {
                input.validValue = false;
                LG.tip(Message);
                if (input.parent()) input.parent().focus();
                input.focus();
                input.select();
            } else {
                if (Message) LG.tip(Message);
                input.validValue = true;
            }
        };
        options.error = function (message) {
            LG.tip(message);
        };
        LG.ajax(options);
    }
    //获取当前页面的MenuNo
    //优先级1：如果页面存在MenuNo的表单元素，那么加载它的值
    //优先级2：加载QueryString，名字为MenuNo的值
    LG.getPageMenuNo = function () {
        var menuno = $("#MenuNo").val();
        if (!menuno) {
            menuno = getQueryStringByName("MenuNo");
        }
        return menuno;
    };

    //创建按钮
    LG.createButton = function (options) {
        var p = $.extend({
            type: "info small",
            width: 60,
            appendTo: $('body')
        }, options || {});
        p.onClick = p.click;
        if (typeof (p.appendTo) == "string") p.appendTo = $(p.appendTo);
        return $('<button></button>').appendTo(p.appendTo).ligerButton(p).button;
    };

    //创建过滤规则(查询表单)
    LG.bulidFilterGroup = function (form) {
        if (!form) return null;
        var group = {
            op: "and",
            rules: []
        };
        group.rules = liger.get(form).toConditions();
        return group;
    };

    //通用上方查询
    LG.commonSearch = function (form, grid, limit, loadServer) {
        if (window["isValid"] !== undefined) window["isValid"] = false;
        grid.isValid = false;
        grid.endEdit();
        grid.isValid = true;
        if (window["isValid"] !== undefined) window["isValid"] = true;
        if (grid.options.url && grid.isDataChanged) {
            $.ligerDialog.confirm(grid.options.isContinueByDataChanged, function (confirm) {
                if (confirm) {
                    f_inner();
                    return true;
                } else {
                    return false;
                }
            });
        } else {
            f_inner();
            return true;
        }

        function f_inner() {
            if (form) {
                var mainform = liger.get(form);
                if (!mainform.valid()) {
                    mainform.showInvalid();
                    return false;
                }
            }
            if (limit) {
                if (!limit(mainform)) return false;
            }
            var rule = LG.bulidFilterGroup(form);
            if (loadServer === false) {
                grid.loadData(false, undefined, rule);
                return;
            }
            var parms = grid.options.parms;
            if (rule.rules.length) {
                if (!grid.options.NoFirstSearch) {
                    grid.options.NoFirstSearch = true;
                    if (!parms.where) parms.where = "{}";
                    parms.orginwhere = parms.where;
                }
                var rules = {
                    groups: [],
                    op: 'and'
                };
                if (parms.orginwhere) rules.groups.push(JSON2.parse(parms.orginwhere));
                rules.groups.push(rule);
                parms.where = JSON2.stringify(rules);
            } else {
                if (grid.options.NoFirstSearch) parms.where = parms.orginwhere;
            }
            if (!parms.where) parms.where = "{}";
            grid.options.newPage = 1;
            grid.loadData();
        }
    }

    //高级过滤
    LG.commonFilter = function (form, grid, limit) {
        if (grid.options.url && grid.isDataChanged && !confirm(grid.options.isContinueByDataChanged))
            return false;
        if (form) {
            var mainform = liger.get(form);
            if (!mainform.valid()) {
                mainform.showInvalid();
                return false;
            }
        }
        if (limit) {
            if (!limit(mainform)) return false;
        }
        grid.showFilterHistory();
    }

    //附加表单搜索按钮：搜索、高级搜索
    LG.appendSearchButtons = function (form, grid, isNotbtn2Container, buttons, limit) {
        if (!form) return;
        form = $(form);
        var jbuttons = $('<div class="l-form-buttons"></div>');
        jbuttons.css({
            "float": "left"
        });
        jbuttons.wrap("<li></li>").parent().appendTo(form.find(".l-form-container:first>ul:last"));
        form.after('<div class="l-clear"></div>');
        LG.addSearchButtons(form, grid, jbuttons, ((isNotbtn2Container != false) ? null : jbuttons), buttons, limit);
    };

    //创建表单搜索按钮：搜索、高级搜索
    LG.addSearchButtons = function (form, grid, btn1Container, btn2Container, buttons, limit) {
        if (!form) return;
        if (btn1Container) {
            var searchButton = LG.createButton({
                appendTo: btn1Container,
                text: '查询',
                click: function () {
                    LG.commonSearch($("#formsearch"), grid, limit);
                }
            });
            if (buttons) buttons.push(searchButton);
        }
        if (btn2Container) {
            var searchButton2 = LG.createButton({
                appendTo: btn2Container,
                width: 80,
                text: '高级搜索',
                click: function () {
                    if (grid.options.url && grid.isDataChanged && !confirm(grid.options.isContinueByDataChanged))
                        return false;
                    if (form) {
                        var mainform = liger.get(form);
                        if (!mainform.valid()) {
                            mainform.showInvalid();
                            return false;
                        }
                    }
                    if (limit) {
                        if (!limit(mainform)) return false;
                    }
                    grid.showFilter();
                }
            });
            if (buttons) buttons.push(searchButton2);
        }
    };

    //快速设置表单底部默认的按钮:保存、取消
    LG.setFormDefaultBtn = function (cancleCallback, savedCallback) {
        //表单底部按钮
        var buttons = [];
        if (cancleCallback) {
            buttons.push({
                text: '取消',
                onclick: cancleCallback
            });
        }
        if (savedCallback) {
            buttons.push({
                text: '保存',
                onclick: savedCallback
            });
        }
        LG.addFormButtons(buttons);
    };

    //增加表单底部按钮,比如：保存、取消
    LG.addFormButtons = function (buttons) {
        if (!buttons) return;
        var formbar = $("body > div.form-bar");
        if (formbar.length == 0)
            formbar = $('<div class="form-bar"><div class="l-dialog-buttons"></div></div>').appendTo('body');
        if (!(buttons instanceof Array)) {
            buttons = [buttons];
        }
        var btnWrap = $("> div:first", formbar);
        $(buttons).each(function (i, item) {
            var btn = $('<button class="l-dialog-btn"></button>');
            item.text = item.text || "BUTTON";
            item.type = item.type || "info";
            btnWrap.append(btn);
            btn.ligerButton(item)
        });
    };

    //提示 验证错误信息
    LG.showInvalid = function (validator) {
        validator = validator || LG.validator;
        if (!validator) return;
        var message = '<div class="invalid">存在' + validator.errorList.length + '个字段验证不通过，请检查!</div>';
        $.ligerDialog.error(message);
    };

    //提示 验证错误信息
    LG.showInvalidTip = function (validator) {
        validator = validator || LG.validator;
        if (!validator) return;
        var message = '<div class="invalid">' + validator.errorList[0].message + '</div>';
        LG.tip(message);
    };
    //表单验证
    LG.validate = function (form, options) {
        if (typeof form === "string" || (typeof form === "object" && form.NodeType == 1)) {
            form = $(form);
        }
        options = $.extend({
            ignore: ":disabled",
            errorPlacement: function (lable, element) {
                var content = $(lable).html();
                var o = liger.get(element) || {};
                var wrapper = o.wrapper || o.text || element;
                wrapper.addClass("l-text-invalid");
                var opt = (element.rules() || {}).tip || {
                    distanceX: 5,
                    distanceY: -3
                };
                if (opt.auto === undefined) opt.auto = true;
                opt.content = content;
                wrapper.ligerHideTip().ligerTip(opt);
            },
            success: function (lable) {
                var eleId = lable.attr("for");
                if (!eleId) return;
                var element = $("#" + eleId);
                var o = liger.get(element) || {};
                var wrapper = o.wrapper || o.text || element;
                wrapper.removeClass("l-text-invalid");
                wrapper.ligerHideTip();
            }
        }, options || {});
        LG.validator = form.validate(options);
        return LG.validator;
    };

    LG.loadToolbar = function (grid, toolbarBtnItemClick, callback, isLine) {
        if (!grid.toolbarManager) return;
        var toolbarOptions = grid.toolbarManager.options;
        var MenuNo = LG.getPageMenuNo();
        LG.ajax({
            loading: '正在加载工具条中...',
            url: "Auth/MyButtons",
            data: {
                menuNo: MenuNo,
                group: toolbarOptions.group,//按钮组
                prefix: toolbarOptions.prefix//按钮前缀
            },
            success: function (data) {
                if (!data || !data.length) {
                    grid.set({
                        "toolbar": null
                    });
                    if (callback) callback(grid.toolbarManager);
                    grid._onResize(); //2014年9月19日 加入防止toolbar还没生成就有高度
                    return;
                };
                var items = [];
                for (var i = 0, l = data.length; i < l; i++) {
                    var o = data[i];
                    items[items.length] = {
                        parentGrid: grid,
                        type: "info",
                        cls: o.BTNCLASS || "",
                        click: toolbarBtnItemClick,
                        text: (o.BTNNAME || "").split("_")[0],
                        icon: o.BTNICON,
                        id: o.BTNNO
                    };
                    if (isLine) items[items.length] = {
                        line: true
                    };
                }
                grid.set({
                    "toolbar": {
                        "items": items
                    }
                });
                if (callback) callback(grid.toolbarManager);
                grid._onResize(); //2014年9月19日 加入防止toolbar还没生成就有高度
            }
        });
    };

    //覆盖页面grid的loading效果
    LG.overrideGridLoading = function () {
        $.extend($.ligerDefaults.Grid, {
            onloading: function () {
                LG.showLoading('正在加载表格数据中...');
            },
            onloaded: function () {
                LG.hideLoading();
            }
        });
    };

    //根据字段权限调整 页面配置
    LG.adujestConfig = function (config, forbidFields) {
        if (config.Form && config.Form.fields) {
            for (var i = config.Form.fields.length - 1; i >= 0; i--) {
                var field = config.Form.fields[i];
                if ($.inArray(field.name, forbidFields) != -1)
                    config.Form.fields.splice(i, 1);
            }
        }
        if (config.Grid && config.Grid.columns) {
            for (var i = config.Grid.columns.length - 1; i >= 0; i--) {
                var column = config.Grid.columns[i];
                if ($.inArray(column.name, forbidFields) != -1)
                    config.Grid.columns.splice(i, 1);
            }
        }
        if (config.Search && config.Search.fields) {
            for (var i = config.Search.fields.length - 1; i >= 0; i--) {
                var field = config.Search.fields[i];
                if ($.inArray(field.name, forbidFields) != -1)
                    config.Search.fields.splice(i, 1);
            }
        }
    };

    //查找是否存在某一个按钮
    LG.findToolbarItem = function (grid, itemID) {
        if (!grid.toolbarManager) return null;
        if (!grid.toolbarManager.options.items) return null;
        var items = grid.toolbarManager.options.items;
        for (var i = 0, l = items.length; i < l; i++) {
            if (items[i].id == itemID) return items[i];
        }
        return null;
    }

    //设置grid的双击事件(带权限控制)
    LG.setGridDoubleClick = function (grid, btnID, btnItemClick) {
        btnItemClick = btnItemClick || toolbarBtnItemClick;
        if (!btnItemClick) return;
        grid.bind('dblClickRow', function (rowdata) {
            var item = LG.findToolbarItem(grid, btnID);
            if (!item) return;
            grid.select(rowdata);
            btnItemClick(item);
        });
    }

    LG.addDays = function (now) {
        if (!now) return now;
        for (var i = 1, l = arguments.length; i < l; i++) {
            var add = arguments[i];
            add = add * 24 * 60 * 60 * 1000;
            now.setTime(now.getTime() + add);
        }
        return now;
    };

    LG.truncDate = function (now, format) {
        format = (format || "").toLowerCase();
        switch (format) {
            case "mi":
                format = "yyyy-MM-dd HH:mm";
                break;
            case "hh":
            case "hh24":
                format = "yyyy-MM-dd HH";
                break;
            case "mm":
                format = "yyyy-MM";
                break;
            case "yyyy":
            case "yy":
                format = "yyyy";
                break;
            default:
                format = "yyyy-MM-dd";
        }
        return now.format(format).toDate(format);
    };

    LG.getSysdate = function (dataSend, dataRev) {
        var oldData = dataSend;
        if (dataSend.idfield) {
            dataSend = { DateFormat: "yyyy-MM-dd HH:mm:ss", DateAddType: "year", DateAdd: 0.001 };
        }
        var options = {
            async: false,
            url: "common/sysdate",
            data: dataSend,
            success: function (data, message) {
                dataRev.ID = data.Sysdate.toDate();
                dataRev.TEXT = data.Adddate.toDate();
                var inner = function (now, field) {
                    now = now.replace(/-/g, " + -").replace(/\+/g, ",").replace(/trunc/ig, "LG.truncDate");
                    now = field + " = LG.addDays(" + now.replace(/sysdate/ig, field) + ");";
                    eval(now);
                };
                if (oldData.idfield) {
                    inner(oldData.idfield, "dataRev.ID");
                }
                if (oldData.textfield) {
                    inner(oldData.textfield, "dataRev.TEXT");
                }
            }
        };
        LG.ajax(options);
    }

    //上传文件
    LG.showUpload = function (options) {
        var p = options || {};
        $.ligerDialog.open({
            height: 350,
            width: 550,
            title: p.title || '文件上传',
            url: p.url || (window.gksybConfigs.urlBase + "fileoper/uploadfile.html"),
            isHidden: false,
            showMax: false,
            showToggle: false,
            showMin: false,
            isResize: true,
            slide: false,
            data: p
        });
    }

    //下载文件
    LG.download = function (p) {
        var url = p.url || p.server || "";
        if (url.indexOf(".") >= 0) {
            window.open(url);
            return;
        }
        p.url = url;
        $.ajax($.extend(true, {
            type: 'post',
            async: true,
            xhrFields: {
                responseType: 'blob'
            },
            success: function (data, status, xhr) {
                var name = xhr.getResponseHeader("Content-disposition");
                var match = name.match(new RegExp("filename\\*=[^\&]+\'\'([^\&]+)", "i"));
                if (!match || match.length < 1) {
                    match = name.match(new RegExp("filename=([^\&]+);", "i"));
                }
                if (!match || match.length < 1) {
                    LG.showError("文件名获取失败");
                    return;
                }
                saveAs(data, decodeURIComponent(match[1]));
            }
        }, p));
    }

    //form拼合数据并返回那些列被更新了
    //currentData 当前数据 orginData 原始数据 bolComb 是否组合当前数据和原始数据
    LG.getFormCombChanges = function (currentData, orginData, bolComb) {
        var updateFields = [];
        var temp1, temp2;
        if (bolComb) {
            for (var k in orginData) {
                if (currentData[k] == undefined) {
                    currentData[k] = orginData[k];
                }
            }
        }
        for (var k in currentData) {
            temp1 = (currentData[k] || "");
            temp2 = (orginData[k] || "");
            if (!(temp1 >= temp2 && temp1 <= temp2)) //判断等于 对于日期要这样判断
                updateFields.push(k);
        }
        return updateFields.join(",");
    }
    //后台数据日期格式化
    LG.formatDate = function (data) {
        if (Object.prototype.toString.apply(data) === '[object Array]') {
            for (var i in data) {
                LG.formatDate(data[i]);
            }
        } else if (typeof data === 'object') {
            for (var i in data) {
                if (/^\/Date/.test(data[i])) {
                    data[i] = data[i].toDate();
                }
            }
        }
    }
    //跳转明细
    LG.toDetail = function (title, url, target, options) {
        if (!url) return;
        url = encodeURI(window.gksybConfigs.getUrl(url, window.gksybConfigs.urlBase));
        if (target && typeof target === "string") {
            window.open(url, target);
            return;
        }
        if (topWindow.f_addTab) {
            topWindow.f_addTab(undefined, title, url);
            return;
        }
        var dialog = topWindow.$.ligerDialog || window.$.ligerDialog;
        if (dialog) {
            dialog.open($.extend({
                width: 800,
                height: 600,
                title: title,
                url: url,
                allowClose: true,
                isHidden: false,
                showMax: false,
                showToggle: false,
                showMin: false,
                isResize: true,
                slide: false
            }, options || target));
            return;
        }
        window.open(url, "_blank");
    }
})(jQuery);