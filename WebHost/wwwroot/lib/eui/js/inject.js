(function ($) {
    /*
    以html的方式加载组件
    程序会查询以 liger-插件名 类名的Dom,从dom加载相应的参数并调用插件
    比如遇到 .liger-grid 的dom，会找到 liger.defaults.Grid 加载需要的参数
    而在config.Grid中配置了这些参数的类型,会动态得加载data,而columns会设置为数组
    参数处理的优先级为：
    1,ignores 忽略不处理的参数
    2,dom存在 {属性名} 的类名 ,比如 <ul class="columns"></ul> ,便会将这个参数设置为复杂属性(object或array):找到相应的defaults和config来加载
    defaults是先找$.liger.inject.defaults,找不到再找liger.defaults的
    config为{父配置}.{属性名},比如 config.Grid.columns
    3,直接读取 data-{属性名} 或者 {属性名} 的dom属性
    */
    liger.inject = {
        prev: 'liger-',

        /*
        命名规则：插件名_属性名(包括第N级的属性) (插件名首字母大写,属性名首字母小写)
        获取规则：获取default时会先找这里,找不到再找liger.defaults,比如 liger.defaults.Grid_columns
        备注：这里只定义了参数的列表
        */
        defaults: {
            Grid_detail: {
                height: null,
                onShowDetail: null
            },
            Grid_editor: 'ComboBox,DateEditor,Spinner,TextBox,PopupEdit,CheckBoxList,RadioList,Grid_editor',
            Grid_popup: 'PopupEdit',
            Grid_grid: 'Grid',
            Grid_condition: 'Form',
            Grid_toolbar: 'Toolbar',
            Grid_fields: 'Form_fields',
            Form_editor: 'ComboBox,DateEditor,Spinner,TextBox,PopupEdit,CheckBoxList,RadioList,Form_editor',
            Form_grid: 'Grid',
            Form_columns: 'Grid_columns',
            Form_condition: 'Form',
            Form_popup: 'PopupEdit',
            Form_buttons: 'Button',
            Portal_panel: 'Panel'
        },
        /*
        config里面配置了某插件参数或者复杂属性参数的类型(动态加载、数组、默认参数)
        */
        config: {
            Grid: {
                //动态
                dynamics: 'data,isChecked,detail,rowDraggingRender,toolbar,columns',
                //数组
                arrays: 'columns',
                //复杂属性columns
                columns: {
                    dynamics: 'render,totalSummary,headerRender,columns,editor,columns',
                    arrays: 'columns',
                    textProperty: 'display',
                    columns: 'liger.inject.config.Grid.columns',
                    editor: {
                        dynamics: 'data,columns,render,renderItem,grid,condition,ext',
                        grid: 'liger.inject.config.Grid',
                        condition: 'liger.inject.config.Form'
                    }
                },
                toolbar: {
                    arrays: 'items'
                }
            },
            Form: {
                dynamics: 'validate,fields,buttons',
                arrays: 'fields,buttons',
                fields: {
                    textProperty: 'label',
                    dynamics: 'validate,editor',
                    editor: {
                        dynamics: 'data,columns,render,renderItem,grid,condition,attr',
                        grid: 'liger.inject.config.Grid',
                        condition: 'liger.inject.config.Form'
                    }
                },
                buttons: 'liger.inject.config.Button'
            },
            PopupEdit: {
                dynamics: 'grid,condition'
            },
            Button: {
                textProperty: 'text',
                dynamics: 'click'
            },
            ComboBox: {
                dynamics: 'columns,data,tree,grid,condition,render,parms,renderItem'
            },
            ListBox: {
                dynamics: 'columns,data,render,parms'
            },
            RadioList: {
                dynamics: 'data,parms'
            },
            CheckBoxList: {
                dynamics: 'data,parms'
            },
            Panel: {
            },
            Portal: {
                //动态
                dynamics: 'rows,columns',
                //数组
                arrays: 'rows,columns',
                //复杂属性 columns
                columns: {
                    dynamics: 'panels',
                    arrays: 'panels'
                },
                //复杂属性 rows
                rows: {
                    dynamics: 'panels',
                    arrays: 'panels'
                },
                toolbar: {
                    arrays: 'items'
                }
            }
        }
    };
})(jQuery);