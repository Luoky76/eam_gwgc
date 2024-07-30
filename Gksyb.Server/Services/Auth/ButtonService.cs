using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Microsoft.Extensions.Options;

namespace Gksyb.Server.Services.Auth
{
    /// <summary>
    /// 按钮服务
    /// </summary>
    public class ButtonService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly SysContextOptions _options;
        private readonly IRoleModuleService _roleModuleService;

        /// <summary>
        /// 菜单服务
        /// </summary>
        public ButtonService(IDbContext dbContext, IOptions<SysContextOptions> options, IRoleModuleService roleModuleService)
        {
            _dbContext = dbContext;
            _options = options.Value;
            _roleModuleService = roleModuleService;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            return await _dbContext.Query<SYS_BUTTON>().GetGridData(request);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> Save(SaveRequest<SYS_BUTTON> request)
        {
            DateTime? sysdate = await _dbContext.GetSysdate();
            return await _dbContext.SaveEntityAnsyc(request,
                c => new { c.BTNNAME, c.BTNNO, c.BTNCLASS, c.BTNICON, c.BTNSCRIPT, c.MENUNO, c.INITSTATUS, c.SEQNO, c.APPNAME },
                c => a => a.BTNID == c.BTNID
                , BeforeAdd, BeforeUpdate, BeforeDelete, false, null, AfterSave);
        }

        /// <summary>
        /// 批量增加
        /// </summary>
        /// <param name="menuNo"></param>
        /// <param name="appname"></param>
        /// <returns></returns>
        public async Task<AjaxResult> BatchAdd(string menuNo, string appname)
        {
            await _dbContext.UseTransactionAsync(async () =>
            {
                await _dbContext.InsertAsync(() => new SYS_BUTTON()
                {
                    APPNAME = appname,
                    MENUNO = menuNo,
                    BTNNAME = "增加",
                    BTNNO = "add",
                    BTNICON = "fa fa-plus",
                    SEQNO = 20
                });
                await _dbContext.InsertAsync(() => new SYS_BUTTON()
                {
                    APPNAME = appname,
                    MENUNO = menuNo,
                    BTNNAME = "修改",
                    BTNNO = "modify",
                    BTNICON = "fa fa-eraser",
                    SEQNO = 30
                });
                await _dbContext.InsertAsync(() => new SYS_BUTTON()
                {
                    APPNAME = appname,
                    MENUNO = menuNo,
                    BTNNAME = "删除",
                    BTNNO = "delete",
                    BTNICON = "fa fa-times",
                    SEQNO = 40
                });
                await _dbContext.InsertAsync(() => new SYS_BUTTON()
                {
                    APPNAME = appname,
                    MENUNO = menuNo,
                    BTNNAME = "保存",
                    BTNNO = "save",
                    BTNICON = "fa fa-save",
                    SEQNO = 50
                });
            });
            await _roleModuleService.Clear(_options.RoleAppName, appname);
            return AjaxResult.Success("保存成功");
        }

        /// <summary>
        /// 清空按钮
        /// </summary>
        /// <param name="menuNo"></param>
        /// <param name="appname"></param>
        /// <returns></returns>
        public async Task<AjaxResult> Clear(string menuNo, string appname)
        {
            var row = await _dbContext.DeleteAsync<SYS_BUTTON>(c => c.APPNAME == appname && c.MENUNO == menuNo);
            await _roleModuleService.Clear(_options.RoleAppName, appname);
            return AjaxResult.Success(row, "清空成功");
        }

        /// <summary>
        /// 获取去重列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> DistinctListAsync(GridRequest request)
        {
            return await _dbContext.Query<SYS_BUTTON>()
                .GroupBy(x => new {
                    x.BTNNO,
                    x.BTNNAME,
                    x.BTNICON,
                    x.BTNCLASS,
                    x.BTNSCRIPT
                })
                .Select(x => new
                {
                    USE_COUNT = Sql.Count(),
                    x.BTNNO,
                    x.BTNNAME,
                    x.BTNICON,
                    x.BTNCLASS,
                    x.BTNSCRIPT
                })
                .Where(x => x.USE_COUNT > 1)
                .GetGridData(request);
        }

        /// <summary>
        /// 新增前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(SYS_BUTTON entity)
        {
            entity.MENUNO.CheckNotNullOrWhiteSpace("菜单编号");
            entity.APPNAME.CheckNotNullOrWhiteSpace("应用名称");
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(SYS_BUTTON entity)
        {
            entity.MENUNO.CheckNotNullOrWhiteSpace("菜单编号");
            entity.APPNAME.CheckNotNullOrWhiteSpace("应用名称");
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(SYS_BUTTON entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新角色缓存
        /// </summary>
        /// <returns></returns>
        private async Task AfterSave(List<SYS_BUTTON> adds, List<SYS_BUTTON> updates, List<SYS_BUTTON> deletes)
        {
            var appname = adds.Select(c => c.APPNAME).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(appname)) appname = updates.Select(c => c.APPNAME).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(appname)) appname = deletes.Select(c => c.APPNAME).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(appname)) return;
            await _roleModuleService.Clear(_options.RoleAppName, appname);
        }
    }
}