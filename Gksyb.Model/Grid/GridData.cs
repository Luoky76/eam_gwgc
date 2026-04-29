using Gksyb.Common.Json;
using Gksyb.Common.Static;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

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
        [JsonConverter(typeof(GridRowJsonConverter))]
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

    class GridRowJsonConverter : LargeCollectionJsonConverter
    {
        private static readonly int NullValueIgnoreThreshold;
        static GridRowJsonConverter()
        {
            NullValueIgnoreThreshold = HttpContext.Configuration.GetValue($"{OptionName.SysContext}:GridNullValueIgnoreThreshold", defaultValue: 21);
        }
        public GridRowJsonConverter() : base(NullValueIgnoreThreshold)
        {
        }
    }
}