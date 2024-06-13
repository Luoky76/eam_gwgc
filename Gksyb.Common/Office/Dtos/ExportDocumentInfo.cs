using System.Data;

namespace Gksyb.Common.Office.Core
{
    public class ExportDocumentInfoOfListData<T> : ExportDocumentInfoBase where T : class
    {
        public ExportDocumentInfoOfListData(ICollection<T> datas) : base(typeof(T))
        {
            Datas = datas;
        }

        /// <summary>
        /// 数据
        /// </summary>
        public ICollection<T> Datas { get; set; }

        /// <summary>
        /// 数据转DataTable
        /// </summary>
        /// <returns></returns>
        public DataTable ToDataTable()
        {
            return Datas.ToDataTable();
        }
    }

    public class ExportDocumentInfo<T> : ExportDocumentInfoBase where T : class
    {
        public ExportDocumentInfo(T data) : base(typeof(T))
        {
            Data = data;
        }

        public ExportDocumentInfo(T data, Type type) : base(type)
        {
            Data = data;
        }

        /// <summary>
        /// 数据
        /// </summary>
        public T Data { get; set; }
    }

    public class ExportDocumentInfoBase
    {
        public ExportDocumentInfoBase(Type type)
        {
            Title = type.GetAttribute<ExporterAttribute>()?.Name ?? type.Name;
            Headers = type.GetProperties().Select(c =>
            {
                var exporterHeader = c.GetAttribute<ExporterHeaderAttribute>() ?? new ExporterHeaderAttribute();
                if (string.IsNullOrEmpty(exporterHeader.DisplayName))
                {
                    exporterHeader.DisplayName = c.GetDisplayName() ?? c.Name;
                }
                return exporterHeader;
            }).ToList();
        }

        /// <summary>
        /// 文档标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 头部信息
        /// </summary>
        public List<ExporterHeaderAttribute> Headers { get; set; }
    }
}