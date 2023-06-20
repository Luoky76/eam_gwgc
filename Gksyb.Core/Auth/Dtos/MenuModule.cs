namespace Gksyb.Core.Auth
{
    public class MenuModule
    {
        /// <summary>
        /// 菜单ID
        /// </summary>
        public long? MENUID { get; set; }

        /// <summary>
        /// 菜单号
        /// </summary>
        public string MENUNO { get; set; }

        /// <summary>
        /// 父菜单号
        /// </summary>
        public string MENUPARENTNO { get; set; }

        /// <summary>
        /// 菜单序号
        /// </summary>
        public int? MENUORDER { get; set; }

        /// <summary>
        /// 菜单名称
        /// </summary>
        public string MENUNAME { get; set; }

        /// <summary>
        /// 菜单链接
        /// </summary>
        public string MENUURL { get; set; }

        /// <summary>
        /// 菜单图标
        /// </summary>
        public string MENUICON { get; set; }

        /// <summary>
        /// 是否可见
        /// </summary>
        public int? ISVISIBLE { get; set; }

        /// <summary>
        /// 是否叶节点
        /// </summary>
        public int? ISLEAF { get; set; }

        /// <summary>
        /// 程序名
        /// </summary>
        public string APPNAME { get; set; }
    }
}