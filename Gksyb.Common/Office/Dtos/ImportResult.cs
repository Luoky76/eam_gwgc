namespace Gksyb.Common.Office.Core
{
    public class ImportResultEnumer<T> where T : class
    {
        /// <summary>
        /// </summary>
        public ImportResultEnumer()
        {
            RowErrors = new List<DataRowErrorInfo>();
        }

        /// <summary>
        /// 导入数据
        /// </summary>
        public virtual IEnumerable<T> Data { get; set; }

        /// <summary>
        /// 验证错误
        /// </summary>
        public virtual IList<DataRowErrorInfo> RowErrors { get; set; }

        /// <summary>
        /// 模板错误
        /// </summary>
        public virtual IList<TemplateErrorInfo> TemplateErrors { get; set; }

        /// <summary>
        /// 导入异常信息
        /// </summary>
        public virtual Exception Exception { get; set; }

        /// <summary>
        /// 是否存在导入错误
        /// </summary>
        public virtual bool HasError => Exception != null ||
                                        (TemplateErrors?.Count(p => p.ErrorLevel == ErrorLevels.Error) ?? 0) > 0 ||
                                        (RowErrors?.Count ?? 0) > 0;

        /// <summary>
        /// Imported header list information
        /// 导入的表头列表信息
        /// https://github.com/dotnetcore/Magicodes.IE/issues/76
        /// </summary>
        public virtual IList<ImporterHeaderInfo> ImporterHeaderInfos { get; set; }
    }

    /// <summary>
    /// 导入结果
    /// </summary>
    public class ImportResult<T> : ImportResultEnumer<T> where T : class
    {
        /// <summary>
        /// </summary>
        public ImportResult() : base()
        {
        }

        public new ICollection<T> Data
        {
            get { return (ICollection<T>)base.Data; }
            set { base.Data = value; }
        }
    }
}