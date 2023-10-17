using Chloe;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Grid;
using Gksyb.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gksyb.Core.Grid;
using EAM.Special.Interfaces;
using Gksyb.Common;
using NPOI.SS.Formula.PTG;
using Microsoft.AspNetCore.Http;
using Gksyb.Common.Office;
using EAM.Special.DTO;
using Gksyb.Model.UI;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Gksyb.Common.Data;
using Gksyb.Core.Auth;
using Gksyb.Model.Core;

namespace EAM.Special.Services
{
    public class BuildService : IBuildService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly IUserService _userService;
        private readonly UserSession _userSession;

        public BuildService(IDbContext dbContext, IComboxDataService comboxDataService, IUserService userService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userService = userService;
            _userSession=  userSession;
        }

        /// <summary>
        /// 下拉
        /// </summary>
        /// <returns></returns>
        public async Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData()
        {
            return await _comboxDataService.Get(new Dictionary<string, object>(){
                { "ShipInfo",null},
            });
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<BUILD_COUNT>()
                .LeftJoin<DEVICE_CARD>((a, b) => a.DEVICE_ID == b.DEVICE_ID)
                .Select((a, b) => new
                {
                    b.SEC_DEPTID,
                    a.BUILD_ID,
                    b.DEPT_ID,
                    a.DEVICE_ID,
                    a.DEVICE_NAME,
                    a.STARTDATE,
                    a.SHIPTIMES,
                    a.SHIPNUM,
                    a.CONPLAN,
                    a.DREDGETIME,
                    a.SAILTIME,
                    a.REPAIRTIME,
                    a.WEATHEREFFECT,
                    a.OTHERSTOP,
                    a.DAILYCONSUMPTION,
                    a.SUPPLEMENT,
                    a.STOCK,
                    a.MASTER,
                    a.AUXILIARY,
                    a.PUMP,
                    a.SUBTOTAL,
                    a.SUPPLEMENT2,
                    a.STOCK2,
                    a.LUBRICATE,
                    a.MEMO
                })
                .WhereIf(!_userSession.IsAdmin, a => _userSession.ParentCompany.CorpID == a.DEPT_ID)
                .GetGridData(request);
            return list;
        }

        public async Task<AjaxResult> GetAsync(string ID)
        {
            var list = await _dbContext.Query<BUILD_COUNT>(x => x.BUILD_ID == ID).ToListAsync();

            return AjaxResult.Success(list);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> Save(SaveRequest<BUILD_COUNT> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.BUILD_ID,
                    c.DEVICE_ID,
                    c.DEVICE_NAME,
                    c.STARTDATE,
                    c.SHIPTIMES,
                    c.SHIPNUM,
                    c.CONPLAN,
                    c.DREDGETIME,
                    c.SAILTIME,
                    c.REPAIRTIME,
                    c.WEATHEREFFECT,
                    c.OTHERSTOP,
                    c.DAILYCONSUMPTION,
                    c.SUPPLEMENT,
                    c.STOCK,
                    c.MASTER,
                    c.AUXILIARY,
                    c.PUMP,
                    c.SUBTOTAL,
                    c.SUPPLEMENT2,
                    c.STOCK2,
                    c.LUBRICATE,
                    c.MEMO
                },
                c => a => a.BUILD_ID == c.BUILD_ID, BeforeAdd, BeforeUpdate, BeforeDelete, false);
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        private async Task BeforeAdd(BUILD_COUNT entity)
        {
            entity.BUILD_ID = GuidHelper.NewSnowflakeId().ToString();
            var isex = await _dbContext.Query<BUILD_COUNT>(x => x.STARTDATE == entity.STARTDATE).ToListAsync();
            if (isex.Count() > 0)
            {
                throw new MessageException("已存在此日期数据，无法重复添加！");
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(BUILD_COUNT request)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeDelete(BUILD_COUNT request)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        /// <exception cref="MessageException"></exception>
        public async Task<AjaxResult> ImportAsync([FileOptions("xlsx,xls", 1)] IFormFile formFile)
        {
            try
            {
                _dbContext.Session.BeginTransaction();
                //获取导入数据
                await formFile.Import<BuildImportDto>(async c =>
                {
                    //判断是否已存在此日期
                    var data = await _dbContext.Query<BUILD_COUNT>()
                    .Where(a => a.STARTDATE == c.STARTDATE)
                    .FirstOrDefaultAsync();

                    if (data != null)
                    {
                        throw new MessageException("已存在"+ c.STARTDATE.ToString("yyyy-MM-dd") + "日期数据，无法重复添加！");
                    }

                    if (string.IsNullOrWhiteSpace(c.DEVICE_NAME))
                    {
                        throw new MessageException("船舶名称不能为空！");
                    }

                    var device = await _dbContext.Query<DEVICE_CARD>(x => x.DEVICE_NAME == c.DEVICE_NAME).FirstOrDefaultAsync();

                    BUILD_COUNT dto = c.MapTo<BUILD_COUNT>();
                    dto.BUILD_ID = GuidHelper.NewSnowflakeId().ToString();
                    dto.DEVICE_ID = device.DEVICE_ID;

                    await _dbContext.InsertAsync<BUILD_COUNT>(dto);

                });
                _dbContext.Session.CommitTransaction();
            }
            catch (Exception e)
            {
                _dbContext.Session.RollbackTransaction();
                throw new MessageException(e.Message);
            }

            return AjaxResult.Success(1);
        }

        /// <summary>
        /// 年份查询
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> QryYearAsync(DateTime year)
        {
            return await _dbContext.Query<BUILD_COUNT>()
                .Where(x => x.STARTDATE.Year == year.Year)
                .GetGridData(null);
        }
        
        /// <summary>
         /// 导出模板数据
         /// </summary>
         /// <returns></returns>
        public async Task<GridData> ExportListAsync(string year)
        {
            var res = await _dbContext.Query<BUILD_COUNT>()
                .Where(x => x.STARTDATE.Year.Equals(year))
                .Select(t => new BuildExportData
                {
                    DEVICE_NAME = t.DEVICE_NAME,
                    SHIPTIMES = t.SHIPTIMES,
                    ZYTIME = t.DREDGETIME + t.SAILTIME,
                    STOPTIME = t.STOPTIME,
                    DAILYCONSUMPTION = t.DAILYCONSUMPTION,
                    MASTER = t.MASTER,
                    AUXILIARY = t.AUXILIARY,
                    LUBRICATE = t.LUBRICATE,
                    PUMP = t.PUMP,
                })
                .GetGridData(null);
            return res;
        }
    }
}
