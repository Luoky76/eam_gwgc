using Flurl.Http;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Gksyb.Server.Services.Auth
{
    /// <summary>
    /// 菜单服务
    /// </summary>
    public class MenuService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly SysContextOptions _options;
        private readonly IRoleModuleService _roleModuleService;
        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// 菜单服务
        /// </summary>
        public MenuService(IDbContext dbContext, IOptions<SysContextOptions> options, IRoleModuleService roleModuleService, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _options = options.Value;
            _roleModuleService = roleModuleService;
            _environment = environment;
        }

        /// <summary>
        /// 获取树形结构
        /// </summary>
        /// <param name="appname"></param>
        /// <returns></returns>
        public async Task<AjaxResult> TreeAsync(string appname)
        {
            appname = string.IsNullOrWhiteSpace(appname) ? _options.AppName : appname;
            var list = await _dbContext.Query<SYS_MENU>().Where(c => c.APPNAME == appname).OrderBy(c => c.MENUORDER).ToListAsync();
            var data = list.Select(c => new { ID = c.MENUNO, TEXT = c.MENUNAME, PARENTID = string.IsNullOrWhiteSpace(c.MENUPARENTNO) ? "ROOT" : c.MENUPARENTNO, ICON = c.MENUICON, c.APPNAME })
                           .ToList();
            data.Add(new { ID = "ROOT", TEXT = "主菜单", PARENTID = "", ICON = "fa fa-folder-open", APPNAME = appname });
            return AjaxResult.Success(data);
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            return await _dbContext.Query<SYS_MENU>().GetGridData(request);
        }

        private static readonly Regex ICONIFY_REGEX = new (@"[a-z0-9]+(?:-[a-z0-9]+)*:[a-z0-9]+(?:-[a-z0-9]+)*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// 生成图标
        /// </summary>
        public async Task GenerateAsync(string appname)
        {
            appname = string.IsNullOrWhiteSpace(appname) ? _options.AppName : appname;
            var list = await _dbContext.Query<SYS_MENU>().Where(c => c.APPNAME == appname).Select(c=>c.MENUICON).ToListAsync();
            var icons = list.Select(c=> ICONIFY_REGEX.Matches(c).Cast<Match>().Select(m => m.Value).FirstOrDefault()).DistinctAndOrderBy().ToList();
            if (icons.Count < 1)
            {
                return;
            }
            var basePath = Path.Combine(_environment.WebRootPath, "vben", "iconify");
            if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
            foreach (var icon in icons)
            {
                var infos = icon.Split(':');
                var prefix = infos[0];
                var name = infos[1];
                var filePath = Path.Combine(basePath, $"{prefix}-{name}.json");
                if (File.Exists(filePath))
                {
                    continue;
                }
                var content = await $"https://api.iconify.design/{prefix}.json?icons={name}".GetBytesAsync();
                await File.WriteAllBytesAsync(filePath, content);
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> Save(SaveRequest<SYS_MENU> request)
        {
            DateTime? sysdate = await _dbContext.GetSysdate();
            return await _dbContext.SaveEntityAnsyc(request,
                c => new { c.MENUNO, c.MENUPARENTNO, c.MENUORDER, c.MENUNAME, c.MENUURL, c.MENUICON, c.ISVISIBLE, c.ISLEAF, c.APPNAME },
                c => a => a.MENUID == c.MENUID
                , BeforeAdd, BeforeUpdate, BeforeDelete, false, null, AfterSave);
        }

        /// <summary>
        /// 新增前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(SYS_MENU entity)
        {
            if (string.IsNullOrWhiteSpace(entity.APPNAME)) entity.APPNAME = _options.AppName;
            if (await _dbContext.Query<SYS_MENU>().Where(c => c.APPNAME == entity.APPNAME && c.MENUNO == entity.MENUNO).AnyAsync())
            {
                throw new MessageException($"菜单编号{entity.MENUNO}已存在");
            }
        }

        /// <summary>
        /// 更新前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(SYS_MENU entity)
        {
            var modelSysMenu = await _dbContext.Query<SYS_MENU>().Where(c => c.MENUID == entity.MENUID).FirstAsync();
            if (modelSysMenu.MENUNO != entity.MENUNO)
            {
                if (await _dbContext.Query<SYS_MENU>().Where(c => c.APPNAME == entity.APPNAME && c.MENUNO == entity.MENUNO).AnyAsync())
                {
                    throw new MessageException($"菜单编号{entity.MENUNO}已存在");
                }
                await _dbContext.UpdateAsync<SYS_MENU>(c => c.APPNAME == entity.APPNAME && c.MENUPARENTNO == modelSysMenu.MENUNO, c => new SYS_MENU()
                {
                    MENUPARENTNO = entity.MENUNO
                });
                await _dbContext.UpdateAsync<SYS_BUTTON>(c => c.APPNAME == entity.APPNAME && c.MENUNO == modelSysMenu.MENUNO, c => new SYS_BUTTON()
                {
                    MENUNO = entity.MENUNO
                });
            }
        }

        /// <summary>
        /// 删除前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(SYS_MENU entity)
        {
            var modelSysMenu = await _dbContext.Query<SYS_MENU>().Where(c => c.MENUID == entity.MENUID).FirstAsync();
            if (await _dbContext.Query<SYS_MENU>().Where(c => c.MENUPARENTNO == modelSysMenu.MENUNO && c.APPNAME == modelSysMenu.APPNAME).AnyAsync())
            {
                throw new MessageException($"菜单编号{modelSysMenu.MENUNO}存在子菜单");
            }
            if (await _dbContext.Query<SYS_BUTTON>().Where(c => c.MENUNO == modelSysMenu.MENUNO && c.APPNAME == modelSysMenu.APPNAME).AnyAsync())
            {
                throw new Exception($"菜单编号{modelSysMenu.MENUNO}存在按钮，请先清除按钮");
            }
        }

        /// <summary>
        /// 更新角色缓存
        /// </summary>
        /// <returns></returns>
        private async Task AfterSave(List<SYS_MENU> adds, List<SYS_MENU> updates, List<SYS_MENU> deletes)
        {
            var appname = adds.Select(c => c.APPNAME).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(appname)) appname = updates.Select(c => c.APPNAME).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(appname)) appname = deletes.Select(c => c.APPNAME).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(appname)) return;
            await _roleModuleService.Clear(_options.RoleAppName, appname);
        }
    }
}