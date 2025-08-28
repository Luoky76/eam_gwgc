using Chloe;
using EAM.Device.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Device.Services
{
    public class DeviceCardService : IDeviceCardService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly IUserService _userService;
        private readonly ICorpService _corpService;

        public DeviceCardService(IDbContext dbContext, IComboxDataService comboxDataService, IUserService userService, ICorpService corpService)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userService = userService;
            _corpService = corpService;
        }

        #region 设备卡片

        public async Task<AjaxResult> ComboxData()
        {
            try
            {
                var data = await _comboxDataService.Get(new Dictionary<string, object>()
                {
                    { "Auditing", null },
                    { "User", null },
                    { "BCCode", "deviceType" },
                });
                //data.TryAdd("User", await _userService.ComboxDataAsync());
                data.TryAdd("Corp", await _corpService.ComboxDataAsync());

                return AjaxResult.Success(data);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }

        /// <summary>
        /// 获取树形结构
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> TreeAsync()
        {
            var composeData = await _dbContext.Query<DEVICE_CARD>(c => c.TYPE_ID == "1").ToListAsync();
            var deviceList = composeData.Select(c => new
            {
                c.DEVICE_NAME,
                c.DEVICE_ID,
                c.DEPT_ID,
                c.DEPT_NAME,
                TYPE = "1",
                PARENTID = "ROOT",
                ICON = "fa fa-group"
            }).OrderBy(c => c.DEVICE_ID).ToList();

            var typeData = await _dbContext.Query<DEVICE_CARD>().Where(c => c.TYPE_ID == "2").ToListAsync();
            var typeList = typeData.Select(c => new
            {
                c.DEVICE_NAME,
                c.DEVICE_ID,
                DEPT_ID = c.DEPT_ID + c.DEVICE_ID,
                c.DEPT_NAME,
                TYPE = "0",
                PARENTID = c.DEPT_ID,
                ICON = "fa fa-cog"
            }).ToList();

            deviceList = deviceList.Concat(typeList).ToList();

            deviceList.Add(new
            {
                DEVICE_NAME = "船舶",
                DEVICE_ID = "ROOT",
                DEPT_ID = "ROOT",
                DEPT_NAME = "船舶",
                TYPE = "-1",
                PARENTID = "",
                ICON = "fa fa-sitemap"
            });
            return AjaxResult.Success(deviceList, "成功");
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<DEVICE_CARD>().GetGridData(request);
            return list;
        }

        /// <summary>
        /// 保存
        /// </summary>
        public async Task<AjaxResult> SaveAsync(SaveRequest<DEVICE_CARD> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.DEVICE_ID,
                    c.DEVICE_NO,
                    c.DEVICE_NAME,
                    c.TYPE_NAME,
                    c.TYPE_ID,
                    c.BOM_NAME,
                    c.DEVICE_TYPE,
                    c.DEPT_ID,
                    c.WDEPT_ID,
                    c.CARD_USERID,
                    c.SHIP_ID,
                    c.INSTALL_SITE,
                    c.SHIP_LENGTH,
                    c.SHIP_WIDTH,
                    c.SHIP_DEPTH,
                    c.SHIP_DRAFT,
                    c.SHIP_TYPE,
                    c.MAIN_POWER,
                    c.SECOND_POWER,
                    c.SPEED,
                    c.DRAG_FORCE,
                    c.TOTAL_TON,
                    c.PURE_TON,
                    c.REV_DATE,
                    c.AUDITING,
                    c.DEPT_NAME,
                    c.WDEPT_NAME,
                    c.CARD_DATE,
                    c.STATUS,
                    c.PROVIDER_NAME,
                    c.OUT_CODE,
                    c.OUT_DATE,
                    c.GRAPH_NO,
                    c.PURPOSE,
                    c.FACTORY,
                    c.STATUS_DATE,
                    c.REP_DATE,
                    c.IS_GREEN,
                    c.ASSET_CODE,
                    c.DEP_MONTH,
                    c.DEP_VALUE,
                    c.PRICE,
                    c.ORG_VALUE,
                    c.NET_VALUE,
                    c.USE_YEAR,
                    c.NET_RATE,
                    c.DEP_RATE,
                    c.ASSET_SOURCE,
                    c.INSTALL_FEE,
                    c.PARAMS,
                    c.MEMO,
                    c.FINAN_TYPE,
                    c.REP_DATE5,
                    c.CARD_USER
                },
                c => a => a.DEVICE_ID == c.DEVICE_ID
                , BeforeAdd, BeforeUpdate, BeforeDelete);
        }

        /// <summary>
        /// 获取单行数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<AjaxResult> GetAsync(string id)
        {
            var query = await _dbContext.Query<DEVICE_CARD>().Where(c => c.DEVICE_ID == id).ToListAsync();

            return AjaxResult.Success(query);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(DEVICE_CARD entity)
        {
            entity.DEVICE_ID = GuidHelper.NewSnowflakeId().ToString();
            if (entity.TYPE_ID == "2")
            {
                entity.STATUS = "1";
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(DEVICE_CARD entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(DEVICE_CARD entity)
        {
            await Task.CompletedTask;
        }

        #endregion

        #region 设备参数

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ParamListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<DEVICE_PARAM>().GetGridData(request);
            return list;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveParamAsync(SaveRequest<DEVICE_PARAM> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.DEVICE_ID,
                    c.PARAM_NAME,
                    c.PARAM_VALUE,
                    c.PARAM_MEMO,
                    c.PARAM_CODE,
                    c.PARAM_SUB,
                    c.TECHFUN_NAME,
                    c.PARAM_ID
                },
                c => a => a.PARAM_ID == c.PARAM_ID
                , BeforeAdd, BeforeUpdate, BeforeDelete);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(DEVICE_PARAM entity)
        {
            entity.PARAM_ID = GuidHelper.NewSnowflakeId().ToString();

            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(DEVICE_PARAM entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(DEVICE_PARAM entity)
        {
            await Task.CompletedTask;
        }
        #endregion

        #region 设备随机资料

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> DocListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<DEVICE_DOC>().GetGridData(request);
            return list;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveDocAsync(SaveRequest<DEVICE_DOC> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.DEVICE_ID,
                    c.DOC_ID,
                    c.DOC_CODE,
                    c.DOC_NAME,
                    c.DOC_NUM,
                    c.DOC_SITE,
                    c.DOC_ATTACH,
                    c.DOC_EXPLAIN
                },
                c => a => a.DOC_ID == c.DOC_ID
                , BeforeAdd, BeforeUpdate, BeforeDelete);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(DEVICE_DOC entity)
        {
            entity.DOC_ID = GuidHelper.NewSnowflakeId().ToString();

            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(DEVICE_DOC entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(DEVICE_DOC entity)
        {
            await Task.CompletedTask;
        }

        #endregion

        #region 重大改造履历

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> RemListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<DEVICE_REMOULD>().GetGridData(request);
            return list;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveRemAsync(SaveRequest<DEVICE_REMOULD> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.DEVICE_ID,
                    c.REMOULD_TYPE,
                    c.REMOULD_DESC,
                    c.ISFINISH,
                    c.REMARK,
                    c.START_DATE,
                    c.END_DATE,
                    c.DEVICE_REMOULD_ID
                },
                c => a => a.DEVICE_REMOULD_ID == c.DEVICE_REMOULD_ID
                , BeforeAdd, BeforeUpdate, BeforeDelete);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(DEVICE_REMOULD entity)
        {
            entity.DEVICE_REMOULD_ID = GuidHelper.NewSnowflakeId().ToString();

            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(DEVICE_REMOULD entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(DEVICE_REMOULD entity)
        {
            await Task.CompletedTask;
        }

        #endregion

        #region 设备台账

        public async Task<GridData> DeviceListAllAsync(GridRequest request)
        {
            var list = await _dbContext.Query<DEVICE_CARD>()
                .LeftJoin<DEVICE_DOC>((a, b) => a.DEVICE_ID == b.DEVICE_ID)
                .LeftJoin<DEVICE_PARAM>((a, b, c) => a.DEVICE_ID == c.DEVICE_ID)
                .LeftJoin<DEVICE_REMOULD>((a, b, c, d) => a.DEVICE_ID == d.DEVICE_ID)
                .Where((a, b, c, d) => a.AUDITING == "1" || a.AUDITING == "3")
                .Select((a, b, c, d) => new
                {
                    a.MEMO
                }).GetGridData(request);

            return list;
        }

        #endregion

        #region 维保设备

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> PmListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<PM_PLAN_EXE>().GetGridData(request);
            return list;
        }

        #endregion
    }
}