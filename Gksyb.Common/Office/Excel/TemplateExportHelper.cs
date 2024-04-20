using Gksyb.Common.Office.Core;
using Magicodes.IE.EPPlus;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using System.Collections;
using System.Dynamic;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using System.Web;

namespace Gksyb.Common.Office.Excel
{
    /// <summary>
    /// 模板导出辅助类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TemplateExportHelper<T> : IDisposable where T : class
    {
        /// <summary>
        /// 变量正则
        /// </summary>
        private static readonly Regex _variableRegex = new("(\\{\\{)+([\\w_.>|\\?:&=]*)+(\\}\\})", RegexOptions.IgnoreCase);

        private static readonly Regex _scriptRegex = new(@"\{\{(.+?)\}\}");

        /// <summary>
        /// 模板写入器
        /// </summary>
        private Dictionary<string, List<Writer>> SheetWriters { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        protected T Data { get; set; }

        public bool IsExpandoObjectType
        {
            get
            {
                if (isExpandoObjectType.HasValue) return isExpandoObjectType.Value;
                isExpandoObjectType = typeof(T) == typeof(ExpandoObject);
                return isExpandoObjectType.Value;
            }
        }

        private bool? isExpandoObjectType;

        /// <summary>
        /// 根据模板导出Excel
        /// </summary>
        /// <param name="path">模板文件路径</param>
        /// <param name="data"></param>
        /// <param name="callback">完成导出后执行的操作，默认导出无操作</param>
        public async Task Export(string path, T data, Action<ExcelPackage> callback = null)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("模板文件路径不能为空!", nameof(path));
            using var stream = new FileStream(path, FileMode.Open);
            await Export(stream, data, callback);
        }

        /// <summary>
        /// 根据模板导出Excel
        /// </summary>
        /// <param name="templateStream">模板文件流</param>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        /// <exception cref="ArgumentException">完成导出后执行的操作，默认导出无操作</exception>
        public async Task Export(Stream templateStream, T data, Action<ExcelPackage> callback)
        {
            Data = data ?? throw new ArgumentException("数据不能为空!", nameof(data));
            using var excelPackage = new ExcelPackage(templateStream);
            ParseTemplateFile(excelPackage);
            await ParseData(excelPackage);
            callback?.Invoke(excelPackage);
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="excelPackage"></param>
        private async Task ParseData(ExcelPackage excelPackage)
        {
            //TODO:渲染支持自定义处理程序
            foreach (var sheetName in SheetWriters.Keys)
            {
                var sheet = excelPackage.Workbook.Worksheets[sheetName];

                //渲染表格
                await RenderTable(sheet);

                //处理普通单元格模板
                await RenderCells(sheet);

                //重新设置行宽（适应图片）
                RenderRowsHeight(sheet);
            }
        }

        /// <summary>
        /// 处理普通单元格模板
        /// </summary>
        /// <param name="sheet"></param>
        private async Task RenderCells(ExcelWorksheet sheet)
        {
            var tableData = new List<object> { Data };
            var tableInfo = new TemplateTableInfo()
            {
                TableData = new List<object> { Data },
                TableKey = "",
                RawRowStart = 0,
                NewRowStart = 0,
                RowCount = tableData.Count
            };
            foreach (var writer in SheetWriters[sheet.Name].Where(p => p.WriterType == WriterTypes.Cell))
            {
                var address = new ExcelAddressBase(writer.TplAddress);
                tableInfo.NewRowStart = writer.RowIndex - address.Start.Row;
                await RenderTableCells(sheet, tableInfo, writer, address);
            }
        }

        /// <summary>
        /// 渲染表格
        /// </summary>
        private async Task RenderTable(ExcelWorksheet sheet)
        {
            var tableGroups = SheetWriters[sheet.Name].Where(p => p.WriterType == WriterTypes.Table)
                .GroupBy(p => p.TableKey);

            var insertRows = 0;
            //支持一行多表格
            //1）获取所有表格的区域范围（列数行数以及坐标）
            var tableInfoList = new List<TemplateTableInfo>(tableGroups.Count());
            var query = new List<object> { Data }.AsQueryable();
            foreach (var tableGroup in tableGroups)
            {
                var tableKey = tableGroup.Key;
                var tableData = query.Select(tableKey).FirstOrDefault() as IEnumerable;
                var startCol = tableGroup.OrderBy(p => p.ColIndex).First();
                var rowStart = startCol.RowIndex;
                var tableInfo = new TemplateTableInfo()
                {
                    TableData = tableData,
                    TableKey = tableKey,
                    RawRowStart = rowStart,
                    NewRowStart = rowStart,
                    RowCount = tableData.AsQueryable().Count(),
                    Writers = tableGroup
                };
                tableInfoList.Add(tableInfo);
            }

            var rowTableGroups = tableInfoList.GroupBy(p => p.RawRowStart);
            foreach (var item in rowTableGroups)
            {
                //是否为一行多个Table
                var isManyTable = item.Count() > 1;
                //一行多Table以最大的为准
                TemplateTableInfo table = !isManyTable ? item.First() : item.OrderByDescending(p => p.RowCount).First();

                if (table.RowCount == 0)
                    continue;

                if (isManyTable)
                {
                    foreach (var itemTable in item)
                    {
                        itemTable.NewRowStart += insertRows;
                    }
                }
                else
                    table.NewRowStart += insertRows;

                //2）统一插入行
                var startRow = table.NewRowStart;
                //插入行
                //插入的目标行号
                var targetRow = table.NewRowStart + 1;
                //插入
                var numRowsToInsert = table.RowCount - 1;
                var refRow = table.NewRowStart;

                if (numRowsToInsert == 0) continue;
                sheet.InsertRow(targetRow, numRowsToInsert);
                //EPPlus的问题。修复如果存在合并的单元格，但是在新插入的行无法生效的问题，具体见 https://stackoverflow.com/questions/31853046/epplus-copy-style-to-a-range/34299694#34299694

                var maxCloumn = sheet.Dimension.End.Column;
                RowCopy(sheet, refRow, refRow, table.RowCount, maxCloumn);

                #region 更新单元格

                var updateCellWriters = SheetWriters[sheet.Name].Where(p => p.WriterType == WriterTypes.Cell).Where(p => p.RowIndex > table.RawRowStart);
                foreach (var writer in updateCellWriters)
                {
                    writer.RowIndex += table.RowCount - 1;
                }

                #endregion 更新单元格

                //表格渲染完成后更新插入的行数
                insertRows += table.RowCount - 1;
            }

            //4）渲染表格
            foreach (var table in tableInfoList)
            {
                var tableGroup = table.Writers;
                var tableKey = tableGroup.Key;
                foreach (var col in tableGroup)
                {
                    var address = new ExcelAddressBase(col.TplAddress);
                    if (table.RowCount == 0)
                    {
                        sheet.Cells[address.Start.Row, address.Start.Column].Value = string.Empty;
                        continue;
                    }
                    await RenderTableCells(sheet, table, col, address);
                }
            }
        }

        private async Task RenderTableCells(ExcelWorksheet sheet, TemplateTableInfo info, Writer writer, ExcelAddressBase address)
        {
            var scripts = writer.CellScript;
            var query = IsExpandoObjectType ? (info.TableData as IEnumerable<object>).AsQueryable() : info.TableData.AsQueryable();
            var insertRows = info.NewRowStart - info.RawRowStart;
            var rowCount = query.Count();
            var datas = new Dictionary<string, List<dynamic>>();
            //渲染一列单元格
            for (var i = 0; i < rowCount; i++)
            {
                var rowIndex = address.Start.Row + i + insertRows;
                var targetAddress = new ExcelAddress(rowIndex, address.Start.Column, rowIndex, address.Start.Column);
                sheet.Row(rowIndex).Height = sheet.Row(address.Start.Row).Height;
                var cell = sheet.Cells[targetAddress.Address];
                foreach (var script in scripts)
                {
                    object value = string.Empty;
                    if (!string.IsNullOrWhiteSpace(script.Name))
                    {
                        if (!datas.ContainsKey(script.Name))
                        {
                            datas[script.Name] = await query.Select(script.Name).ToDynamicListAsync();
                        }
                        value = datas[script.Name].Skip(i).FirstOrDefault();
                        value = value is JValue raw ? raw.Value : value;
                    }
                    switch (script.Type)
                    {
                        case "image":
                        case "img":
                            value = RenderImageCell(cell, value?.ToString(), script.Params, sheet);
                            break;

                        case "formula":
                            cell.Formula = script.Params;
                            break;
                    }
                    if (!string.IsNullOrWhiteSpace(script.Content))
                    {
                        value = string.Format(script.Content, value ?? "");
                    }
                    if (script.Type != "formula")
                        cell.Value = value;
                }
            }
        }

        private static object RenderImageCell(ExcelRange cell, string value, string param, ExcelWorksheet sheet)
        {
            var @params = HttpUtility.ParseQueryString(param);
            var alt = @params["alt"];
            var isBase64 = value.IsBase64String();
            if (value == null || (!File.Exists(value) && !value.StartsWith("http", StringComparison.OrdinalIgnoreCase) && !isBase64))
                return alt;
            try
            {
                ExcelImage image = null;
                if (value.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    image = value.GetImageByUrl();
                }
                else if (isBase64)
                {
                    image = value.Base64StringToImage();
                }
                else if (File.Exists(value))
                {
                    using Stream imageStream = File.OpenRead(value);
                    image = ExcelImage.Decode(imageStream);
                }
                if (image == null)
                    return alt;
                var width = (@params["w"] ?? @params["width"]).CastTo(image.Height);
                var height = (@params["h"] ?? @params["height"] ?? "0").CastTo(image.Width);
                var xOffset = (@params["x"] ?? @params["XOffset"] ?? "0").CastTo<int>();
                var yOffset = (@params["y"] ?? @params["YOffset"] ?? "0").CastTo<int>();
                var excelImage = sheet.Drawings.AddPicture(Guid.NewGuid().ToString(), image, image.Format);
                var address = new ExcelAddress(cell.Address);
                ////调整对齐
                excelImage.From.ColumnOff = Pixel2MTU(xOffset);
                excelImage.From.RowOff = Pixel2MTU(yOffset);
                excelImage.From.Column = address.Start.Column - 1;
                excelImage.From.Row = address.Start.Row - 1;
                excelImage.SetSize(width, height);
                return string.Empty;
            }
            catch (Exception)
            {
                return alt;
            }
        }

        private static List<ScriptInfo> ToScriptInfo(string script)
        {
            var list = new List<ScriptInfo>();
            if (script.Contains("{{Table>>"))//{{ Table >> BookInfo | RowNo}}
            {
                script = "{{" + script.Split('|')[1];
            }
            else if (script.Contains(">>Table}}"))//{{Remark|>>Table}}
            {
                script = script.Split('|')[0] + "}}";
            }
            var matchs = _scriptRegex.Matches(script);
            if (matchs.Count < 1)
            {
                list.Add(new() { Content = script });
                return list;
            }
            foreach (Match match in matchs.Cast<Match>())
            {
                if (!match.Value.Contains("::")) continue;
                script = script.Replace(match.Value, "");
                var array = match.Groups[1].Value.Split("::");
                var urls = new Regex(@"(\?|\&)").Split(array.Last(), 2);
                var info = new ScriptInfo
                {
                    Type = array.First().ToLower().Trim(),
                    Name = urls.FirstOrDefault(),
                    Params = urls.Last()
                };
                if (info.Type == "formula")
                {
                    info.Params = info.Params.Replace("params=", "").Replace("&", ",");
                    info.Params = $"={info.Name}({info.Params})";
                    info.Name = null;
                }
                else
                {
                    info.Name = SyntaxFacts.IsValidIdentifier(info.Name) ? info.Name : $"it[\"{info.Name}\"]";
                }
                list.Add(info);
            }
            if (string.IsNullOrWhiteSpace(script))
                return list;
            var name = _scriptRegex.Replace(script, match =>
            {
                var pname = match.Groups[1].Value;
                pname = SyntaxFacts.IsValidIdentifier(pname) ? pname : $"it[\"{pname}\"]";
                return $"\",{pname},\"";
            });
            name = name.Replace(",\"\",", ",");
            name = name.StartsWith("\",") ? name.TrimStart("\",".ToCharArray())
                : $"\"{name}";
            name = name.EndsWith(",\"") ? name.TrimEnd(",\"".ToCharArray())
                : $"{name}\"";
            if (name.Contains(','))
                name = $"String.Concat({name})";
            list.Add(new() { Name = name });
            return list;
        }

        /// <summary>
        /// 多行复制
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="startRow">复制前的开始行</param>
        /// <param name="endRow">复制前的结束行</param>
        /// <param name="totalRows">总行数</param>
        /// <param name="maxColumnNum">最大列数</param>
        private static void RowCopy(ExcelWorksheet sheet, int startRow, int endRow, int totalRows, int maxColumnNum)
        {
            //rows表示现有的sheet行数
            int rows = endRow - startRow + 1;
            if (totalRows > rows * 2)
            {
                //行数复制一倍
                sheet.Cells[startRow, 1, endRow, maxColumnNum].Copy(sheet.Cells[endRow + 1, 1, endRow * 2 - startRow + 1, maxColumnNum]);
                //再次循环
                RowCopy(sheet, startRow, endRow * 2 - startRow + 1, totalRows, maxColumnNum);
            }
            else
            {
                //行数复制需要(需要复制 totalRows - rows)
                sheet.Cells[startRow, 1, startRow + (totalRows - rows) - 1, maxColumnNum].Copy(sheet.Cells[endRow + 1, 1, startRow + totalRows, maxColumnNum]);
            }
        }

        /// <summary>
        /// 重新设置行宽（适应图片）
        /// </summary>
        /// <param name="sheet"></param>
        private static void RenderRowsHeight(ExcelWorksheet sheet)
        {
            var rows = new List<int>();
            foreach (var item in sheet.Drawings)
            {
                if (item is ExcelPicture pic)
                {
                    var rowIndex = pic.From.Row + 1;
                    if (rows.Contains(rowIndex))
                        continue;
                    //https://github.com/dotnetcore/Magicodes.IE/issues/131
                    //sheet.Row(rowIndex).Height = pic.Image.Height;
                    sheet.Row(rowIndex).Height = pic.GetPrivateProperty<int>("_height");
                    rows.Add(rowIndex);
                }
            }
            rows.Clear();
        }

        internal static int Pixel2MTU(int pixels)
        {
            int mtus = pixels * 9525;
            return mtus;
        }

        /// <summary>
        /// 验证并转换模板
        /// </summary>
        /// <param name="excelPackage"></param>
        protected void ParseTemplateFile(ExcelPackage excelPackage)
        {
            SheetWriters = new Dictionary<string, List<Writer>>();
            foreach (var worksheet in excelPackage.Workbook.Worksheets)
            {
                if (worksheet.Dimension == null)
                    continue;
                var writers = new List<Writer>();
                if (!SheetWriters.ContainsKey(worksheet.Name)) SheetWriters.Add(worksheet.Name, writers);
                var endColumnIndex = worksheet.Dimension.End.Column;
                var endRowIndex = worksheet.Dimension.End.Row;

                //获取所有包含表达式的单元格
                var rows = worksheet.Cells[worksheet.Dimension.Start.Row, worksheet.Dimension.Start.Column, endRowIndex, endColumnIndex]
                    .Where(c => _variableRegex.IsMatch((c.Value ?? string.Empty).ToString())).GroupBy(c => c.Rows);

                foreach (var rowGroups in rows)
                {
                    var isStartTable = false;
                    string tableKey = null;
                    foreach (var cell in rowGroups)
                    {
                        var cellString = cell.Value.ToString();
                        if (cellString.Contains("{{Table>>"))
                        {
                            isStartTable = true;
                            //{{ Table >> BookInfo | RowNo}}
                            tableKey = Regex.Split(cellString, "{{Table>>")[1].Split('|')[0].Trim();
                        }

                        writers.Add(new Writer
                        {
                            TableKey = tableKey,
                            TplAddress = cell.Address,
                            CellString = cellString,
                            CellScript = ToScriptInfo(cellString),
                            WriterType = isStartTable ? WriterTypes.Table : WriterTypes.Cell,
                            RowIndex = cell.Start.Row,
                            ColIndex = cell.Start.Column
                        });

                        if (isStartTable && cellString.Contains(">>Table}}"))
                        {
                            isStartTable = false;
                            tableKey = null;
                        }
                    }
                }
            }
        }

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
            SheetWriters = null;
            GC.SuppressFinalize(this);
        }
    }
}