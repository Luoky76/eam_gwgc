using Gksyb.Model.UI;
using System.Text.RegularExpressions;

namespace Gksyb.Core.Common
{
    public class QueryView
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string VIEWS { get; set; }

        /// <summary>
        /// 表格js
        /// </summary>
        public string GRID { get; set; }

        /// <summary>
        /// 查询语句
        /// </summary>
        public string SEARCH { get; set; }

        /// <summary>
        /// 表单js
        /// </summary>
        public string FORM { get; set; }

        /// <summary>
        /// 程序名称
        /// </summary>
        public string APPNAME { get; set; }

        /// <summary>
        /// 是否包含变量
        /// </summary>
        public bool HastParmMatch { get; set; }

        /// <summary>
        /// 数据源
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// 字段
        /// </summary>
        public List<KeyValueItem> Fields { get; set; }

        /// <summary>
        /// 数据处理
        /// </summary>
        /// <returns></returns>
        public async Task HandleAsync(IDbContext dbContext)
        {
            HastParmMatch = Regex.IsMatch(SEARCH, @"{(\w+)}");
            if (SEARCH.Contains("DataSource")) SearchHandle();
            await GridHandle(dbContext);
            Fields ??= new List<KeyValueItem>();
        }

        /// <summary>
        ///search处理
        /// </summary>
        /// <returns></returns>
        private void SearchHandle()
        {
            var view = SEARCH;
            int index1 = view.IndexOf(":", view.IndexOf("DataSource"));
            int index2 = view.IndexOf(",", view.IndexOf("DataSource"));
            DataSource = view.Substring(index1 + 1, index2 - index1 - 1);
            SEARCH = view[(index2 + 1)..];
        }

        /// <summary>
        /// Grid处理
        /// </summary>
        /// <returns></returns>
        private async Task GridHandle(IDbContext dbContext)
        {
            IDbContext dbContextLind = await dbContext.GetDbContext(DataSource);
            try
            {
                var search = SEARCH.Split(';').Where(c => (c ?? "").Trim().StartsWith("select", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(search)) return;
                var compare = "'@#' = '@#'";
                search = search.Contains(compare) ? search.Replace(compare, "1 = 2") : $"SELECT * FROM ({search}) tmptablequery WHERE 1 = 2";
                search = Regex.Replace(search, @"{(\w+)}", $"{dbContextLind.GetParamPrefix()}$1");
                search = Regex.Replace(search, $@"{dbContextLind.GetParamRegex()}(\w+)", " null ");
                Fields = new List<KeyValueItem>();
                using var reader = await dbContextLind.Session.ExecuteReaderAsync(search);
                var fieldCount = reader.FieldCount;
                for (int i = 0; i < fieldCount; i++)
                {
                    var fieldType = reader.GetFieldType(i);
                    var value = "string";
                    if (typeof(DateTime).Equals(fieldType))
                    {
                        value = "date";
                    }
                    else if (typeof(int).Equals(fieldType) || typeof(short).Equals(fieldType) || typeof(long).Equals(fieldType))
                    {
                        value = "int";
                    }
                    else if (typeof(float).Equals(fieldType) || typeof(decimal).Equals(fieldType))
                    {
                        value = "float";
                    }
                    Fields.Add(new KeyValueItem()
                    {
                        Key = reader.GetName(i),
                        Value = value
                    });
                }
            }
            finally
            {
                if (dbContextLind != dbContext) dbContextLind.Dispose();
            }
        }
    }
}