#pragma warning disable CA1822 // 将成员标记为 static 会使路由不可访问
using Gksyb.Common.Office;
using Gksyb.Common.Office.Core;
using Gksyb.Common.Weixin;
using Gksyb.Core.Auth;
using Gksyb.Model.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WkHtmlToPdfDotNet;

namespace Gksyb.Server.Controllers.Auth
{
    [GksybAuthorize(IsSuper = true)]
    public class DemoController : BaseController
    {
        public DemoController()
        {
        }

        public AjaxResult ParameterHandle(DemoDto request)
        {
            return AjaxResult.Success(request);
        }

        [JsToken]
        public async Task<AjaxResult> Upload([FileOptions("png", 1)] IFormFile formFile, string folder)
        {
            //await formFile.Import<ExportData>(async c =>
            //{
            //    c.ToJson();
            //    await Task.CompletedTask;
            //});
            var path = await formFile.SaveAs(folder, formFile.FileName, true);
            return AjaxResult.Success(path, path);
        }

        [HttpGet, HttpPost]
        public async Task<FileResult> ExportExcelHeader(string filename)
        {
            return await FileExport.ExportToExcelHeader(new ExportData(), filename);
        }

        [HttpGet, HttpPost]
        public async Task<FileResult> ExportExcelTemplate([FromServices] IDbContext dbContext, [FromServices] IWebHostEnvironment webHostEnvironment, string filename)
        {
            var list = dbContext.Query<SYS_BUTTON>().Select(c => new ExportData
            {
                Name1 = c.BTNNAME,
                Name2 = c.BTNNO
            }).ToList();
            var template = Path.Combine(webHostEnvironment.WebRootPath, "demo", "upload", "template.xlsx");
            return await FileExport.ExportToExcelByTemplate(new ExportTemplateData { Company = "", List = list }, template, filename);
        }

        public async Task<FileResult> ListToExcel([FromServices] IDbContext dbContext, string filename)
        {
            var list = dbContext.Query<SYS_BUTTON>().Select(c => new ExportData
            {
                Name1 = c.BTNNAME,
                Name2 = c.BTNNO
            }).ToList();
            return await FileExport.ExportToExcel(list, filename);
        }

        public async Task<FileResult> DataTableToExcel([FromServices] IDbContext dbContext, string filename)
        {
            var list = dbContext.Query<SYS_BUTTON>().Select(c => new ExportData
            {
                Name1 = c.BTNNAME,
                Name2 = c.BTNNO
            }).ToList();
            return await FileExport.ExportToExcel(list, filename);
        }

        public async Task<FileResult> ExportToWord([FromServices] IDbContext dbContext, [FromServices] IWebHostEnvironment webHostEnvironment, string filename)
        {
            var list = dbContext.Query<SYS_BUTTON>().Select(c => new ExportData
            {
                Name1 = c.BTNNAME,
                Name2 = c.BTNNO
            }).First();
            var template = Path.Combine(webHostEnvironment.WebRootPath, "demo", "upload", "template.cshtml");
            return await FileExport.ExportToWord(list, template, filename);
        }

        public async Task<FileResult> ExportToPdf([FromServices] IDbContext dbContext, [FromServices] IWebHostEnvironment webHostEnvironment, string filename)
        {
            var list = dbContext.Query<SYS_BUTTON>().Select(c => new ExportData
            {
                Name1 = c.BTNNAME,
                Name2 = c.BTNNO
            }).First();
            var template = Path.Combine(webHostEnvironment.WebRootPath, "demo", "upload", "template.cshtml");
            return await FileExport.ExportToPdf(list, template, filename);
        }

        public async Task<FileResult> ExportToHtml([FromServices] IDbContext dbContext, [FromServices] IWebHostEnvironment webHostEnvironment, string filename)
        {
            var list = dbContext.Query<SYS_BUTTON>().Select(c => new ExportData
            {
                Name1 = c.BTNNAME,
                Name2 = c.BTNNO
            }).First();
            var template = Path.Combine(webHostEnvironment.WebRootPath, "demo", "upload", "template.cshtml");
            return await FileExport.ExportToHtml(list, template, filename);
        }


        public async Task<FileResult> ListToPdf([FromServices] IDbContext dbContext, [FromServices] IWebHostEnvironment webHostEnvironment, string filename)
        {
            var list = dbContext.Query<SYS_BUTTON>().Select(c => new ExportData
            {
                Name1 = c.BTNNAME,
                Name2 = c.BTNNO
            }).ToList();
            var template = Path.Combine(webHostEnvironment.WebRootPath, "demo", "upload", "template.xlsx");
            return await FileExport.ExportListToPdf(list, template, filename);
        }

        /// <summary>
        /// 微信支付下单
        /// </summary>
        /// <returns></returns>
        [GksybAuthorize(true)]
        public async Task<AjaxResult> Transactions()
        {
            var openid = string.Empty;
            var user = await HttpContext.GetCurrentUserAsync();
            if (user != null) openid = user.Openid;
            if (string.IsNullOrWhiteSpace(openid)) return AjaxResult.Error("无法获取微信号,请退出后重试");
            var request = new WeixinTransactionsRequest(GuidHelper.NewShortId(), "测试商品", "https://www.baidu.com/", 1, openid, 10);
            var response = await WeixinPayHelper.UnifiedOrder(request);
            //var prepayId = await WeixinPayHelper.Transactions(request);
            return AjaxResult.Success(response);
        }

        /// <summary>
        /// 统一下单 微信回调
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<AjaxResult> TransactionsCallback()
        {
            var response = await HttpContext.UnifiedOrderCallback();
            return AjaxResult.Success(response);
        }

        /// <summary>
        /// jsapi下单 微信回调
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public AjaxResult TransactionsV3Callback([FromBody] WeixinCallbackRequest request)
        {
            var transactionsStatus = request.ToTransactionsStatusResponse();
            return AjaxResult.Success(transactionsStatus);
        }
    }

    public class ExportTemplateData
    {
        public string Company { get; set; }
        public List<ExportData> List { get; set; }
    }

    /// <summary>
    ///     在Excel导出中，Name将为Sheet名称
    ///     在HTML、Pdf、Word导出中，Name将为标题
    /// </summary>
    [ExcelExporter(Name = "通用导出测试", Author = "作者", AutoFitMaxRows = 5000)]
    [ExcelImporter(MaxCount = 50000)]
    [PdfExporter(Orientation = Orientation.Landscape, PaperKind = PaperKind.A4, IsWriteHtml = true, IsEnablePagesCount = false)]
    public class ExportData
    {
        /// <summary>
        /// </summary>
        [Display(Name = "列1")]
        [ImporterHeader(Name = "列1")]
        public string Name1 { get; set; }

        [ExporterHeader(DisplayName = "列2")]
        [ImporterHeader(Name = "列2")]
        public string Name2 { get; set; }

        public string Name3 { get; set; }
        public string Name4 { get; set; }
    }

    public class DemoDto
    {
        [ModelEncrypt]
        public Dictionary<string, object> Data { get; set; }

        [ModelEncrypt]
        public string[] Names { get; set; }

        public List<DemoDto> Parameters { get; set; }
    }
}
#pragma warning restore CA1822 // 将成员标记为 static