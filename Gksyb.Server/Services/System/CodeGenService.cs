using Gksyb.Common.Data;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using Microsoft.AspNetCore.Hosting;
using RazorEngineCore;
using System.Collections;
using IOFile = System.IO.File;

namespace Gksyb.Server.Services.System
{
    public class CodeGenService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;
        private readonly IBCCodeService _codeService;

        public CodeGenService(IDbContext dbContext, IWebHostEnvironment environment, IBCCodeService codeService)
        {
            _dbContext = dbContext;
            _environment = environment;
            _codeService = codeService;
        }

        /// <summary>
        /// 下拉数据
        /// </summary>
        public async Task<List<ComboxData>> LinkDataAsync() => await _dbContext.Query<TDBLINK>().Select(c => new ComboxData()
        {
            ID = c.LINKNAME,
            TEXT = c.LINKNAME + "（" + c.CORPID + ")"
        }).ToListAsync();

        /// <summary>
        /// 列表
        /// </summary>
        public async Task<GridData> LinkListAsync(GridRequest request) => await _dbContext.Query<TDBLINK>().GetGridData(request);

        /// <summary>
        /// 保存
        /// </summary>
        public async Task<AjaxResult> LinkSaveAsync(SaveRequest<TDBLINK> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new { c.LINKNAME, c.CORPID, c.LINKTYPE, c.CONNSTR },
                c => a => a.LINKNAME == c.LINKNAME, orgin: true);
        }

        /// <summary>
        /// 列表
        /// </summary>
        public async Task<IList> ListAsync(string link)
        {
            var dbContext = await _dbContext.GetDbContext(link);
            var list = await dbContext.GetTables();
            list.ForEach(c => c.DataSource = link);
            return list;
        }

        /// <summary>
        /// 获取列信息
        /// </summary>
        public async Task<List<DbColumnInfo>> Columns(DbTableInfo tableInfo)
        {
            var dbContext = await _dbContext.GetDbContext(tableInfo.DataSource);
            return await dbContext.GetTableColumns(tableInfo.Name, tableInfo.Schema);
        }

        /// <summary>
        /// 树形数据
        /// </summary>
        public IList TemplateTree()
        {
            var path = Path.Combine(_environment.WebRootPath, "code-gen", "template");
            var files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
            return files.Select(c => Path.GetFileNameWithoutExtension(c)).Select(c => new
            {
                ID = c,
                TEXT = c,
                PARENTID = "",
                ICON = "fa fa-file"
            }).OrderBy(c => c.ID).ToList();
        }

        /// <summary>
        /// 模板生成内容
        /// </summary>
        public async Task<string> TemplateContentAsync(DbTableInfo tableInfo, string name)
        {
            if (tableInfo.Columns == null)
            {
                var dbContext = await _dbContext.GetDbContext(tableInfo.DataSource);
                tableInfo.Columns = await dbContext.GetTableColumns(tableInfo.Name, tableInfo.Schema);
            }
            var template = await GetTemplate(name);
            return template.Run(c => { c.Model = tableInfo; });
        }

        /// <summary>
        /// 模板生成内容
        /// </summary>
        public async Task<string> TemplateBuildAsync(DbTableInfo tableInfo, string template, string path = null)
        {
            var mapName = tableInfo.Name.ToPascal();
            var htmlName = mapName.ToKebabCase();
            var modules = (tableInfo.Module ?? "Gksyb.Server").Split('.').Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
            tableInfo.Module = modules.ToStr(".");
            var business = modules.Take(2).ToStr(".");
            var module = modules.Skip(2).ToStr(".");
            var lastModule = modules.LastOrDefault();
            var basePath = Path.Combine(_environment.ContentRootPath, $"..{Path.DirectorySeparatorChar}");
            var content = await TemplateContentAsync(tableInfo, template);
            path ??= PathConfig.ContainsKey(template) ? PathConfig[template] :
                (await _codeService.Get("代码生成", template)).Select(c => c.TEXT).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(path)) path = "{modules}/{mapName}.cs";
            path = path.Replace(null, new Dictionary<string, object>() {
                    { "modules",modules.ToStr($"{Path.DirectorySeparatorChar}")},
                    { "business",business},
                    { "module",module.Replace('.', Path.DirectorySeparatorChar)},
                    { "name",tableInfo.Name},
                    { "mapName",mapName},
                    { "htmlName",htmlName},
                    { "lastModule",lastModule}
            }).Replace('/', Path.DirectorySeparatorChar);
            path = Path.Combine(basePath, path);
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            await IOFile.WriteAllTextAsync(path, content, Encoding.UTF8);
            return Path.GetFullPath(path);
        }

        private readonly List<KeyValueItem<IRazorEngineCompiledTemplate<RazorEngineTemplateBase<DbTableInfo>>>> _templates = new();

        /// <summary>
        /// 获取模板
        /// </summary>
        private async Task<IRazorEngineCompiledTemplate<RazorEngineTemplateBase<DbTableInfo>>> GetTemplate(string name)
        {
            var item = _templates.Find(c => c.Key == name);
            if (item != null) return item.Value;
            var path = Path.Combine(_environment.WebRootPath, "code-gen", "template", $"{name}.cshtml");
            var content = await File.ReadAllTextAsync(path);
            var razorEngine = new RazorEngineCore.RazorEngine();
            var template = razorEngine.Compile<RazorEngineTemplateBase<DbTableInfo>>(content, builder =>
            {
                builder.AddAssemblyReferenceByName("System.Linq");
                builder.AddAssemblyReferenceByName("System.Collections");
                builder.AddAssemblyReferenceByName("Gksyb.Common");
            });
            _templates.Add(new KeyValueItem<IRazorEngineCompiledTemplate<RazorEngineTemplateBase<DbTableInfo>>>() { Key = name, Value = template });
            return template;
        }

        private static readonly Dictionary<string, string> PathConfig = new()
        {
            {"model","Gksyb.Model/{modules}/{name}.cs" },
            {"controller","{business}/Controllers/{module}/{mapName}.cs" },
            {"service","{business}/Services/{module}/{mapName}.cs" },
            {"view","WebHost/wwwroot/{lastModule}/{htmlName}.html" },
            {"view-detail","WebHost/wwwroot/{lastModule}/{htmlName}-detail.html" }
        };
    }
}