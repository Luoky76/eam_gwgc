namespace Gksyb.Model.Grid
{
    /// <summary>
    /// 分页请求
    /// </summary>
    public class PageRequest
    {
        /// <summary>
        /// 页数
        /// </summary>
        public int? Page { get; set; }

        /// <summary>
        /// 分页大小
        /// </summary>
        public int? PageSize { get; set; }
    }
}