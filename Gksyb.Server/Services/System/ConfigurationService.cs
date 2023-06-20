using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Microsoft.Extensions.Options;

namespace Gksyb.Server.Services.System
{
    /// <summary>
    /// 配置服务
    /// </summary>
    public class ConfigurationService : IBaseService
    {
        private readonly ICommonService _commonService;
        private readonly IDbContext _dbContext;
        private readonly string _appName;

        /// <summary>
        /// 配置服务
        /// </summary>
        public ConfigurationService(IDbContext dbContext, ICommonService commonService, IOptions<SysContextOptions> options)
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _appName = options.Value.ConfigAppName ?? options.Value.AppName;
        }

        /// <summary>
        /// 获取单行数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CF_CONFIGURATION> GetAsync(long id)
        {
            return await _dbContext.Query<CF_CONFIGURATION>().Where(c => c.CONFIGID == id).FirstAsync();
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            return await _dbContext.Query<CF_CONFIGURATION>()
                .Where(c => c.APPNAME == _appName)
                .LeftJoin<SYS_MENU>((config, menu) => config.VIEWS == menu.MENUNO && config.APPNAME == menu.APPNAME)
                .LeftJoin<SYS_MENU>((config, menu, menuParent) => menu.MENUPARENTNO == menuParent.MENUNO && menu.APPNAME == menuParent.APPNAME)
                .Select((config, menu, menuParent) => new
                {
                    config.CONFIGID,
                    config.VIEWS,
                    menu.MENUURL,
                    menu.MENUNO,
                    menu.MENUNAME,
                    PMENUNAME = menuParent.MENUNAME ?? config.FORM.Substring(0, 200),
                    DESC = (menuParent.MENUNAME ?? config.FORM.Substring(0, 200)) + "->" + menu.MENUNAME
                }).GetGridData(request);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> Save(SaveRequest<CF_CONFIGURATION> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new { c.VIEWS, c.GRID, c.SEARCH, c.FORM },
                c => a => a.CONFIGID == c.CONFIGID
                , BeforeAdd, BeforeUpdate, BeforeDelete);
        }

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> UpdateCacheAsync()
        {
            await _commonService.ClearAsync();
            return AjaxResult.Success();
        }

        /// <summary>
        /// 新增前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(CF_CONFIGURATION entity)
        {
            if (string.IsNullOrWhiteSpace(entity.APPNAME)) entity.APPNAME = _appName;
            if (await _dbContext.Query<CF_CONFIGURATION>().Where(c => c.APPNAME == entity.APPNAME && c.VIEWS == entity.VIEWS).AnyAsync())
            {
                throw new MessageException($"视图{entity.VIEWS}已存在");
            }
            entity.GRID = CryptographyHelper.DecryptFront(entity.GRID);
            entity.SEARCH = CryptographyHelper.DecryptFront(entity.SEARCH);
            entity.FORM = CryptographyHelper.DecryptFront(entity.FORM);
            await _commonService.RemoveCacheAsync(entity.VIEWS);
        }

        /// <summary>
        /// 更新前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(CF_CONFIGURATION entity)
        {
            var modelSysMenu = await _dbContext.Query<CF_CONFIGURATION>().Where(c => c.CONFIGID == entity.CONFIGID).FirstAsync();
            if (modelSysMenu.VIEWS != entity.VIEWS)
            {
                if (await _dbContext.Query<CF_CONFIGURATION>().Where(c => c.APPNAME == entity.APPNAME && c.VIEWS == entity.VIEWS).AnyAsync())
                {
                    throw new MessageException($"视图{entity.VIEWS}已存在");
                }
            }
            entity.GRID = CryptographyHelper.DecryptFront(entity.GRID);
            entity.SEARCH = CryptographyHelper.DecryptFront(entity.SEARCH);
            entity.FORM = CryptographyHelper.DecryptFront(entity.FORM);
            await _commonService.RemoveCacheAsync(entity.VIEWS);
        }

        /// <summary>
        /// 删除前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(CF_CONFIGURATION entity)
        {
            await _commonService.RemoveCacheAsync(entity.VIEWS);
        }
    }
}