using Gksyb.Common.Office.Core;
using OfficeOpenXml;
using System.Data;

namespace Gksyb.Common.Office.Excel
{
    public class ExcelExporter : IExporter, IExportFileByTemplate
    {
        private readonly Action<ExcelPackage> _callback = null;

        public ExcelExporter(Action<ExcelPackage> callback = null)
        {
            _callback = callback;
        }

        private ExcelPackage _excelPackage;
        private bool _isSeparateColumn;
        private bool _isSeparateBySheet;
        private bool _isSeparateByRow;
        private bool _isAppendHeaders;

        /// <inheritdoc/>
        public Task<byte[]> ExportAsByteArray<T>(DataTable dataItems) where T : class, new()
        {
            return ExportAsByteArray(dataItems, typeof(T));
        }

        /// <inheritdoc/>
        public Task<byte[]> ExportAsByteArray(DataTable dataItems, Type type)
        {
            var helper = new ExportHelper<DataTable>(type);
            var max = helper.ExcelExporterSettings.MaxRowNumberOnASheet;
            if (max > 0 && dataItems.Rows.Count > max)
            {
                using (helper.CurrentExcelPackage)
                {
                    var ds = dataItems.SplitDataTable(max);
                    var sheetCount = ds.Tables.Count;
                    for (var i = 0; i < sheetCount; i++)
                    {
                        var sheetDataItems = ds.Tables[i];
                        helper.AddExcelWorksheet();
                        helper.Export(sheetDataItems);
                    }
                    _callback?.Invoke(helper.CurrentExcelPackage);
                    return helper.CurrentExcelPackage.GetAsByteArrayAsync();
                }
            }
            else
            {
                using var ep = helper.Export(dataItems);
                _callback?.Invoke(ep);
                return ep.GetAsByteArrayAsync();
            }
        }

        /// <inheritdoc/>
        public Task<byte[]> ExportAsByteArray<T>(ICollection<T> dataItems) where T : class, new()
        {
            var helper = new ExportHelper<T>();
            var max = helper.ExcelExporterSettings.MaxRowNumberOnASheet;
            if (max > 0 && dataItems.Count > max)
            {
                using (helper.CurrentExcelPackage)
                {
                    var sheetCount = (dataItems.Count / max) + ((dataItems.Count % max) > 0 ? 1 : 0);
                    for (var i = 0; i < sheetCount; i++)
                    {
                        var sheetDataItems = dataItems.Skip(i * max).Take(max).ToList();
                        helper.AddExcelWorksheet();
                        helper.Export(sheetDataItems);
                    }
                    _callback?.Invoke(helper.CurrentExcelPackage);
                    return helper.CurrentExcelPackage.GetAsByteArrayAsync();
                }
            }
            else
            {
                using var ep = helper.Export(dataItems);
                _callback?.Invoke(ep);
                return ep.GetAsByteArrayAsync();
            }
        }

        /// <inheritdoc/>
        public async Task ExportBytesByTemplate<T>(Stream stream, T data, string template) where T : class
        {
            using var helper = new TemplateExportHelper<T>();
            await helper.Export(template, data, (package) =>
            {
                _callback?.Invoke(package);
                package.SaveAs(stream);
            });
        }

        /// <inheritdoc/>
        public async Task<byte[]> ExportBytesByTemplate<T>(T data, string template) where T : class
        {
            using var sr = new MemoryStream();
            await ExportBytesByTemplate(sr, data, template);
            return sr.ToArray();
        }

        /// <inheritdoc/>
        public Task<byte[]> ExportBytesByTemplate(object data, string template, Type type)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<byte[]> ExportHeaderAsByteArray<T>(T type) where T : class, new()
        {
            var helper = new ExportHelper<T>();
            using var ep = helper.ExportHeaders();
            _callback?.Invoke(ep);
            return ep.GetAsByteArrayAsync();
        }

        /// <summary>
        /// 导出excel表头
        /// </summary>
        /// <param name="items">表头数组</param>
        /// <param name="sheetName">工作簿名称</param>
        /// <returns></returns>

        public Task<byte[]> ExportHeaderAsByteArray(string[] items, string sheetName = "导出结果")
        {
            var helper = new ExportHelper<DataTable>();
            var headerList = new List<ExporterHeaderInfo>();
            for (var i = 1; i <= items.Length; i++)
            {
                var item = items[i - 1];
                var exporterHeaderInfo =
                    new ExporterHeaderInfo()
                    {
                        Index = i,
                        DisplayName = item,
                        CsTypeName = typeof(string),
                        PropertyName = item,
                        ExporterHeaderAttribute = new ExporterHeaderAttribute(item) { },
                    };
                headerList.Add(exporterHeaderInfo);
            }

            helper.AddExcelWorksheet(sheetName);
            helper.AddExporterHeaderInfoList(headerList);
            using var ep = helper.ExportHeaders();
            _callback?.Invoke(ep);
            return ep.GetAsByteArrayAsync();
        }

        /// <summary>
        /// 追加数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataItems"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public ExcelExporter Append<T>(ICollection<T> dataItems, string sheetName = null) where T : class, new()
        {
            var helper = _excelPackage == null ? new ExportHelper<T>(sheetName) : new ExportHelper<T>(_excelPackage, sheetName);
            if (_isSeparateColumn || _isSeparateBySheet || _isSeparateByRow)
            {
                var name = helper.ExcelExporterSettings?.Name ?? "导出结果";

                if (_excelPackage?.Workbook.Worksheets.Any(x => x.Name == name) == true)
                {
                    throw new ArgumentNullException($"已经存在该名字的sheet:{name}");
                }
            }

            _excelPackage = helper.Export(dataItems);

            if (_isSeparateColumn)
            {
                helper.CopySheet(0, 1);
                _isSeparateColumn = false;
            }

            if (_isSeparateByRow)
            {
                helper.CopyRows(0, 1, _isAppendHeaders);
            }

            _isSeparateBySheet = false;
            _isSeparateByRow = false;
            _isAppendHeaders = false;
            return this;
        }

        /// <summary>
        /// 分割集合到当前Sheet追加Column
        /// </summary>
        /// <returns></returns>
        public ExcelExporter SeparateByColumn()
        {
            CheckPackage();
            _isSeparateColumn = true;
            return this;
        }

        /// <summary>
        /// 分割多出多个sheet
        /// </summary>
        /// <returns></returns>
        public ExcelExporter SeparateBySheet()
        {
            CheckPackage();
            _isSeparateBySheet = true;
            return this;
        }

        /// <summary>
        /// 追加rows到当前sheet
        /// </summary>
        /// <returns></returns>
        public ExcelExporter SeparateByRow()
        {
            CheckPackage();
            _isSeparateByRow = true;
            return this;
        }

        /// <summary>
        /// 追加表头
        /// </summary>
        /// <returns></returns>
        public ExcelExporter AppendHeaders()
        {
            CheckPackage();
            if (!_isSeparateByRow)
            {
                throw new ArgumentNullException("调用当前方法之前，必须先调用SeparateByRow方法！");
            }
            _isAppendHeaders = true;
            return this;
        }

        /// <summary>
        /// 导出所有的追加数据
        /// </summary>
        /// <returns></returns>
        public Task<byte[]> ExportAppendDataAsByteArray()
        {
            CheckPackage();
            _callback?.Invoke(_excelPackage);
            var bytes = _excelPackage.GetAsByteArrayAsync();
            Reset();
            return bytes;
        }

        private void CheckPackage()
        {
            if (_excelPackage != null) return;
            throw new ArgumentNullException("调用当前方法之前，必须先调用Append方法！");
        }

        private void Reset()
        {
            _excelPackage = null;
            _isSeparateByRow = false;
            _isAppendHeaders = false;
            _isSeparateBySheet = false;
            _isSeparateColumn = false;
        }
    }
}