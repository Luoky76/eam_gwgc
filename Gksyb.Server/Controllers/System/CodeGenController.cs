using Gksyb.Common.Data;
using Gksyb.Core.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using Gksyb.Server.Services.System;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Web;

namespace Gksyb.Server.Controllers.System
{
    /// <summary>
    /// 代码生成
    /// </summary>
    [GksybAuthorize(IsSuper = true)]
    public class CodeGenController : BaseController
    {
        private readonly CodeGenService _service;

        public CodeGenController(CodeGenService service)
        {
            _service = service;
        }

        /// <summary>
        /// 下拉数据
        /// </summary>
        public async Task<AjaxResult> ComboxDataAsync()
        {
            return AjaxResult.Success(new
            {
                LinkData = await _service.LinkDataAsync()
            });
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<AjaxResult> LinkListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.LinkListAsync(request));
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<AjaxResult> LinkSaveAsync(SaveRequest<TDBLINK> request)
        {
            return await _service.LinkSaveAsync(request);
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<AjaxResult> ListAsync(string link)
        {
            return AjaxResult.Success(await _service.ListAsync(link));
        }

        /// <summary>
        /// 获取列信息
        /// </summary>
        public async Task<AjaxResult> ColumnsAsync([FromBody] DbTableInfo tableInfo)
        {
            return AjaxResult.Success(await _service.Columns(tableInfo));
        }

        /// <summary>
        /// 树形数据
        /// </summary>
        public AjaxResult TemplateTree()
        {
            return AjaxResult.Success(_service.TemplateTree());
        }

        /// <summary>
        /// 模板内容
        /// </summary>
        public async Task<AjaxResult> TemplateContentAsync(List<DbTableInfo> tableInfos, string template)
        {
            var codes = new List<KeyValueItem>();
            foreach (var tableInfo in tableInfos)
            {
                codes.Add(new KeyValueItem()
                {
                    Key = tableInfo.Name,
                    Value = HttpUtility.HtmlEncode(await _service.TemplateContentAsync(tableInfo, template))
                });
            }
            return AjaxResult.Success(codes);
        }

        /// <summary>
        /// 模板本地生成
        /// </summary>
        public async Task<AjaxResult> TemplateBuildAsync(List<DbTableInfo> tableInfos, string template)
        {
            var paths = new List<string>();
            foreach (var tableInfo in tableInfos)
            {
                tableInfo.Module = (tableInfo.Module ?? "").Replace("Gksyb.Model", "");
                paths.Add(await _service.TemplateBuildAsync(tableInfo, template));
            }
            return AjaxResult.Success(paths.Take(10).ToStr("<br/>"), "成功");
        }

        /// <summary>
        /// 模板内容下载
        /// </summary>
        public async Task<FileResult> TemplateDownloadAsync(List<DbTableInfo> tableInfos, string template)
        {
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var tableInfo in tableInfos)
                {
                    tableInfo.Module = (tableInfo.Module ?? "").Replace("Gksyb.Model", "");
                    await _service.TemplateBuildAsync(tableInfo, template, null, async (path, content) =>
                    {
                        var codeFile = archive.CreateEntry(path);
                        using var streamWriter = new StreamWriter(codeFile.Open());
                        await streamWriter.WriteAsync(content);
                        return path;
                    });
                }
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            var fileName = tableInfos.Select(c => c.Name).FirstOrDefault() ?? "code";
            return new FileContentResult(memoryStream.ToArray(), "application/zip") { FileDownloadName = $"{fileName}.zip" };
        }

        /// <summary>
        /// 代码生成
        /// </summary>
        public async Task<AjaxResult> CodeContentAsync(List<DbTableInfo> tableInfos)
        {
            var codes = new List<object>();
            foreach (var tableInfo in tableInfos)
            {
                var mapName = tableInfo.Name.ToPascal();
                var htmlName = mapName.ToKebabCase();
                codes.Add(new KeyValueItem()
                {
                    Key = $"{htmlName}.html",
                    Value = HttpUtility.HtmlEncode(await _service.TemplateContentAsync(tableInfo, "view"))
                });
                codes.Add(new KeyValueItem()
                {
                    Key = $"{htmlName}-detail.html",
                    Value = HttpUtility.HtmlEncode(await _service.TemplateContentAsync(tableInfo, "view-detail"))
                });
                codes.Add(new KeyValueItem()
                {
                    Key = $"{tableInfo.Name}.cs",
                    Value = HttpUtility.HtmlEncode(await _service.TemplateContentAsync(tableInfo, "model"))
                });
                codes.Add(new KeyValueItem()
                {
                    Key = $"{mapName}Controller.cs",
                    Value = HttpUtility.HtmlEncode(await _service.TemplateContentAsync(tableInfo, "controller"))
                });
                codes.Add(new KeyValueItem()
                {
                    Key = $"{mapName}Service.cs",
                    Value = HttpUtility.HtmlEncode(await _service.TemplateContentAsync(tableInfo, "service"))
                });
            }
            return AjaxResult.Success(codes);
        }

        /// <summary>
        /// 代码本地生成
        /// </summary>
        public async Task<AjaxResult> CodeBuildAsync(List<DbTableInfo> tableInfos)
        {
            var paths = new List<string>();
            foreach (var tableInfo in tableInfos)
            {
                tableInfo.Module = (tableInfo.Module ?? "").Replace("Gksyb.Model", "");
                paths.Add(await _service.TemplateBuildAsync(tableInfo, "model"));
                paths.Add(await _service.TemplateBuildAsync(tableInfo, "controller"));
                paths.Add(await _service.TemplateBuildAsync(tableInfo, "service"));
                paths.Add(await _service.TemplateBuildAsync(tableInfo, "view"));
                paths.Add(await _service.TemplateBuildAsync(tableInfo, "view-detail"));
            }
            return AjaxResult.Success(paths.Take(10).ToStr("<br/>"), "成功");
        }

        /// <summary>
        /// 代码下载
        /// </summary>
        public async Task<FileResult> CodeDownloadAsync(List<DbTableInfo> tableInfos)
        {
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var tableInfo in tableInfos)
                {
                    tableInfo.Module = (tableInfo.Module ?? "").Replace("Gksyb.Model", "");
                    await _service.TemplateBuildAsync(tableInfo, "model", null, async (path, content) =>
                    {
                        var codeFile = archive.CreateEntry(path);
                        using var streamWriter = new StreamWriter(codeFile.Open());
                        await streamWriter.WriteAsync(content);
                        return path;
                    });
                    await _service.TemplateBuildAsync(tableInfo, "controller", null, async (path, content) =>
                    {
                        var codeFile = archive.CreateEntry(path);
                        using var streamWriter = new StreamWriter(codeFile.Open());
                        await streamWriter.WriteAsync(content);
                        return path;
                    });
                    await _service.TemplateBuildAsync(tableInfo, "service", null, async (path, content) =>
                    {
                        var codeFile = archive.CreateEntry(path);
                        using var streamWriter = new StreamWriter(codeFile.Open());
                        await streamWriter.WriteAsync(content);
                        return path;
                    });
                    await _service.TemplateBuildAsync(tableInfo, "view", null, async (path, content) =>
                    {
                        var codeFile = archive.CreateEntry(path);
                        using var streamWriter = new StreamWriter(codeFile.Open());
                        await streamWriter.WriteAsync(content);
                        return path;
                    });
                    await _service.TemplateBuildAsync(tableInfo, "view-detail", null, async (path, content) =>
                    {
                        var codeFile = archive.CreateEntry(path);
                        using var streamWriter = new StreamWriter(codeFile.Open());
                        await streamWriter.WriteAsync(content);
                        return path;
                    });
                }
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            var fileName = tableInfos.Select(c => c.Name).FirstOrDefault() ?? "code";
            return new FileContentResult(memoryStream.ToArray(), "application/zip") { FileDownloadName = $"{fileName}.zip" };
        }
    }
}