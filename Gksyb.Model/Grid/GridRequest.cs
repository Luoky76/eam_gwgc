using Microsoft.AspNetCore.Mvc;

namespace Gksyb.Model.Grid
{
    /// <summary>
    /// 表格请求
    /// </summary>
    public class GridRequest : PageRequest
    {
        /// <summary>
        /// 视图
        /// </summary>
        [ModelEncrypt]
        public string View { get; set; }

        /// <summary>
        /// 列
        /// </summary>
        public string Columns
        {
            get
            {
                return EncrpyCls;
            }
            set
            {
                EncrpyCls = value;
            }
        }

        /// <summary>
        /// 条件
        /// </summary>
        public string Where
        {
            get
            {
                return EncrpyCondition;
            }
            set
            {
                EncrpyCondition = value;
            }
        }

        /// <summary>
        /// 排序
        /// </summary>
        [ModelEncrypt]
        [SqlFilter(80)]
        public string SortName { get; set; }

        /// <summary>
        /// 排序方向
        /// </summary>
        [ModelEncrypt]
        [SqlFilter(5)]
        public string SortOrder { get; set; }

        /// <summary>
        /// 分组
        /// </summary>
        [ModelEncrypt]
        [SqlFilter(20)]
        public string GroupBy { get; set; }

        /// <summary>
        /// 是否翻页
        /// </summary>
        public string ChangePage { get; set; }

        /// <summary>
        /// 是否查询总数
        /// </summary>
        public bool IsTotal
        {
            get
            {
                return Page.HasValue && PageSize.HasValue && ChangePage != "1";
            }
        }

        /// <summary>
        /// 查询类型
        /// </summary>
        public string QueryType { get; set; }

        /// <summary>
        /// 查询条件
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 列
        /// </summary>
        [ModelEncrypt]
        public string EncrpyCls { get; set; }

        /// <summary>
        /// 条件
        /// </summary>
        [ModelEncrypt]
        public string EncrpyCondition { get; set; }

        /// <summary>
        /// 是否有列
        /// </summary>
        public bool HasColumn
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Columns) && Columns != "*";
            }
        }

        /// <summary>
        /// 是否有排序
        /// </summary>
        public bool HasSort
        {
            get
            {
                return !string.IsNullOrWhiteSpace(SortName);
            }
        }
    }
}