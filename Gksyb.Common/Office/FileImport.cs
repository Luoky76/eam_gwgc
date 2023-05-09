using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Core.Models;
using Magicodes.ExporterAndImporter.Excel;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Gksyb.Common.Office
{
    public static class FileImport
    {
        /// <summary>
        /// excel导入
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="formFile">导入文件对象</param>
        /// <param name="func">委托处理</param>
        /// <param name="importer">导入程序</param>
        /// <returns></returns>
        public static async Task<ICollection<T>> Import<T>(this IFormFile formFile, Func<T, Task> func = null, IImporter importer = null) where T : class, new()
        {
            importer ??= new ExcelImporter();
            using var stream = formFile.OpenReadStream();
            return await stream.Import(importer, func);
        }

        /// <summary>
        /// excel导入
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="filePath">导入文件名</param>
        /// <param name="func">委托处理</param>
        /// <param name="importer">导入程序</param>
        /// <returns></returns>
        public static async Task<ICollection<T>> Import<T>(string filePath, Func<T, Task> func = null, IImporter importer = null) where T : class, new()
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new MessageException($"文件路径不能为空!");
            if (!File.Exists(filePath)) throw new MessageException($"文件{filePath}不存在!");
            importer ??= new ExcelImporter();
            using var stream = new FileStream(filePath, FileMode.Open);
            return await stream.Import(importer, func);
        }

        /// <summary>
        /// excel导入
        /// </summary>
        /// <returns></returns>
        public static async Task<ICollection<T>> Import<T>(this Stream stream, IImporter importer, Func<T, Task> func = null) where T : class, new()
        {
            var importResult = await importer.Import<T>(stream);
            return await importResult.HandleAsync(func);
        }

        /// <summary>
        /// excel导入
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="formFile">导入文件对象</param>
        /// <param name="func">委托处理</param>
        /// <returns></returns>
        public static async Task<Dictionary<string, ImportResult<object>>> ImportMultipleSheet<T>(this IFormFile formFile, Func<Dictionary<string, ImportResult<object>>, Task> func = null) where T : class, new()
        {
            IExcelImporter importer = new ExcelImporter();
            using var stream = formFile.OpenReadStream();
            return await stream.ImportMultipleSheet<T>(importer, func);
        }

        /// <summary>
        /// excel导入多sheet
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="filePath">导入文件名</param>
        /// <param name="func">委托处理</param>
        /// <returns></returns>
        public static async Task<Dictionary<string, ImportResult<object>>> ImportMultipleSheet<T>(string filePath, Func<Dictionary<string, ImportResult<object>>, Task> func = null) where T : class, new()
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new MessageException($"文件路径不能为空!");
            if (!File.Exists(filePath)) throw new MessageException($"文件{filePath}不存在!");
            IExcelImporter importer = new ExcelImporter();
            using var stream = new FileStream(filePath, FileMode.Open);
            return await stream.ImportMultipleSheet<T>(importer, func);
        }

        /// <summary>
        /// excel导入多sheet
        /// </summary>
        /// <returns></returns>
        public static async Task<Dictionary<string, ImportResult<object>>> ImportMultipleSheet<T>(this Stream stream, IExcelImporter importer, Func<Dictionary<string, ImportResult<object>>, Task> func = null) where T : class, new()
        {
            var importDic = await importer.ImportMultipleSheet<T>(stream);
            importDic.Values.ForEach(c => c.Check());
            if (func == null) return importDic;
            await func(importDic);
            return importDic;
        }

        /// <summary>
        /// excel处理
        /// </summary>
        /// <returns></returns>
        public static async Task<ICollection<T>> HandleAsync<T>(this ImportResult<T> importResult, Func<T, Task> func = null) where T : class, new()
        {
            importResult.Check();
            if (func == null) return importResult.Data;
            foreach (var data in importResult.Data)
            {
                await func(data);
            }
            return importResult.Data;
        }

        /// <summary>
        /// 检查状态
        /// </summary>
        /// <returns></returns>
        public static bool Check<T>(this ImportResult<T> importResult) where T : class, new()
        {
            if (importResult.Exception != null) throw new MessageException($"{importResult.Exception}");
            if (importResult.RowErrors.Count > 0) throw new MessageException($"{importResult.RowErrors.Select(c => c.ErrorMessage()).ToStr(Environment.NewLine)}");
            if (importResult.HasError) throw new MessageException($"{importResult.TemplateErrors.Where(c => c.ErrorLevel == ErrorLevels.Error).Select(c => c.ErrorMessage()).ToStr(Environment.NewLine)}");
            if (importResult.Data == null) throw new MessageException($"Excel解析失败");
            return true;
        }

        /// <summary>
        /// 错误标准化
        /// </summary>
        /// <returns></returns>
        private static string ErrorMessage(this DataRowErrorInfo dataRowErrorInfo)
        {
            return $"第[{dataRowErrorInfo.RowIndex}]行  {dataRowErrorInfo.FieldErrors.Select(c => $"[{c.Key}]:{c.Value}").ToStr(",")}";
        }

        /// <summary>
        /// 错误标准化
        /// </summary>
        /// <returns></returns>
        private static string ErrorMessage(this TemplateErrorInfo templateErrorInfo)
        {
            return $"[{templateErrorInfo.RequireColumnName ?? templateErrorInfo.ColumnName}]{templateErrorInfo.Message}";
        }
    }
}