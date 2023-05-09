namespace Gksyb.Model.Grid
{
    /// <summary>
    /// 表格数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GridData<T>
    {
        /// <summary>
        /// 数据行
        /// </summary>
        public T Rows { get; set; }

        /// <summary>
        /// 总行数
        /// </summary>
        public int? Total { get; set; }
    }

    /// <summary>
    /// 表格数据
    /// </summary>
    public class GridData : GridData<object>
    {
    }
}