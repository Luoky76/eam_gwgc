using Gksyb.Common.Data;
using Gksyb.Core.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using Gksyb.Server.Services.System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
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
        /// 模型生成
        /// </summary>
        public async Task<AjaxResult> ModelContentAsync(List<DbTableInfo> tableInfos)
        {
            return await TemplateContentAsync(tableInfos, "model");
        }

        /// <summary>
        /// 模型本地生成
        /// </summary>
        public async Task<AjaxResult> ModelBuildAsync([FromServices] IWebHostEnvironment env, List<DbTableInfo> tableInfos)
        {
            return await TemplateBuildAsync(env, tableInfos, "model");
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
        public async Task<AjaxResult> TemplateBuildAsync([FromServices] IWebHostEnvironment env, List<DbTableInfo> tableInfos, string template)
        {
            if (!env.IsDevelopment()) return AjaxResult.Error("只能在本地开发时使用");
            var paths = new List<string>();
            foreach (var tableInfo in tableInfos)
            {
                tableInfo.Module = (tableInfo.Module ?? "").Replace("Gksyb.Model", "");
                paths.Add(await _service.TemplateBuildAsync(tableInfo, template));
            }
            return AjaxResult.Success(paths.Take(10).ToStr("<br/>"), "成功");
        }

        /// <summary>
        /// 服务生成
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
        /// 模型本地生成
        /// </summary>
        public async Task<AjaxResult> CodeBuildAsync([FromServices] IWebHostEnvironment env, List<DbTableInfo> tableInfos)
        {
            if (!env.IsDevelopment()) return AjaxResult.Error("只能在本地开发时使用");
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
    }
}