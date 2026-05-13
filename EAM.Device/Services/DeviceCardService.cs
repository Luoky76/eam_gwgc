using Chloe;
using Gksyb.Common;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Core.Auth;

namespace EAM.Device.Services
{
    public class DeviceCardService : IBaseService
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

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public async Task<AjaxResult> ComboxDataAsync()
        {
            var data = await _comboxDataService.Get(new Dictionary<string, object>()
            {
                { "BCCode@#DeviceType", "deviceType" },
            });
            data.TryAdd("Corp", await _corpService.ComboxDataAsync());
            return AjaxResult.Success(data);
        }

        /// <summary>
        /// 获取树形结构
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> TreeAsync()
        {
            var deviceList = await _dbContext.Query<DEVICE_CARD>()
                .Select(c => new
                {
                    c.DEVICE_ID,
                    c.DEVICE_NAME,
                    c.DEVICE_NO,
                    c.TREE_NODE,
                    PARENTID = string.IsNullOrWhiteSpace(c.PRE_DEVICE_ID) ? "ROOT" : c.PRE_DEVICE_ID,
                    ICON = "fa fa-cog"
                })
                .ToListAsync();
            deviceList.Add(new
            {
                DEVICE_ID = "ROOT",
                DEVICE_NAME = "船舶",
                DEVICE_NO = "",
                TREE_NODE = "",
                PARENTID = "",
                ICON = "fa fa-sitemap"
            });
            return AjaxResult.Success(deviceList, "成功");
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
                c => new { c.PRE_DEVICE_ID, c.TREE_NODE, c.DEVICE_NO, c.DEVICE_NAME, c.BOM_ID, c.BOM_NAME, c.BOM_CODE, c.TYPE_ID, c.TYPE_NAME, c.TYPE_CODE, c.DEVICE_TYPE, c.DEVICE_SIZE, c.CARD_DATE, c.DEPT_ID, c.DEPT_NAME, c.INSTALL_SITE, c.REV_DATE, c.FDEVICE_ID, c.FDEVICE_CODE, c.SPECIAL_TYPE, c.ABC_TYPE, c.LABEL_CODE, c.STATUS, c.STATUS_DATE, c.ASSET_CODE, c.FINAN_TYPE, c.DEP_MONTH, c.DEP_VALUE, c.INSTALL_FEE, c.PRICE, c.ORG_VALUE, c.NET_VALUE, c.USE_YEAR, c.NET_RATE, c.DEP_RATE, c.ASSET_SOURCE, c.PROVIDER_ID, c.PROVIDER_NAME, c.FACTORY, c.OUT_CODE, c.OUT_DATE, c.PURPOSE, c.GRAPH_NO, c.PARAMS, c.MEMO, c.CARD_USERID, c.CARD_USER, c.INSTALL_ID, c.BOX_DET_ID, c.USED_COUNT, c.SHIP_LENGTH, c.SHIP_WIDTH, c.SHIP_DEPTH, c.SHIP_DRAFT, c.SHIP_TYPE, c.MAIN_POWER, c.SECOND_POWER, c.REP_DATE, c.SHIP_ID, c.SPEED, c.DRAG_FORCE, c.TOTAL_TON, c.PURE_TON, c.ORDINAL, c.IS_GREEN, c.REP_DATE5 },
                c => a => a.DEVICE_ID == c.DEVICE_ID,
                BeforeAdd, BeforeUpdate, BeforeDelete, orgin: true);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(DEVICE_CARD entity)
        {
            if (entity.DEVICE_ID.IsNullOrWhiteSpace())
            {
                entity.DEVICE_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            if (entity.TYPE_ID == "2")
            {
                entity.STATUS = "1";
            }
            await Handle(entity);
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(DEVICE_CARD entity)
        {
            await Handle(entity);
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

        /// <summary>
        /// 检查和预处理
        /// </summary>
        private async Task Handle(DEVICE_CARD entity)
        {
            await UpdateCodeCascade(entity);
        }

        /// <summary>
        /// 逐级更新编码
        /// </summary>
        private async Task UpdateCodeCascade(DEVICE_CARD entity)
        {
            //确定当前节点的编码
            if (!entity.PRE_DEVICE_ID.IsNullOrWhiteSpace())
            {
                var preDevice = await _dbContext.QueryByKeyAsync<DEVICE_CARD>(entity.PRE_DEVICE_ID);
                //编码正确，无需重编
                if (!entity.TREE_NODE.IsNullOrWhiteSpace() && entity.TREE_NODE.Length >= 3 && entity.TREE_NODE[..^3] == preDevice.TREE_NODE) return;
                //为当前节点生成编码
                var cur_code = await _dbContext.Query<DEVICE_CARD>(x => x.TREE_NODE.StartsWith(preDevice.TREE_NODE) && x.TREE_NODE.Length == preDevice.TREE_NODE.Length + 3)
                    .MaxAsync(x => x.TREE_NODE);
                entity.TREE_NODE = cur_code.IsNullOrWhiteSpace()
                   ? $"{preDevice.TREE_NODE}001"
                   : $"{cur_code[..^3]}{long.Parse(cur_code[^3..]) + 1:D3}";
            }
            else
            {
                //根节点
                if (entity.TREE_NODE.IsNullOrWhiteSpace())
                {
                    //为根节点生成编码
                    var cur_code = await _dbContext.Query<DEVICE_CARD>(x => string.IsNullOrWhiteSpace(x.PRE_DEVICE_ID))
                        .MaxAsync(x => x.TREE_NODE);
                    entity.TREE_NODE = cur_code.IsNullOrWhiteSpace() ? "001" : $"{long.Parse(cur_code) + 1:D3}";
                }
                else
                {
                    var curDevice = await _dbContext.QueryByKeyAsync<DEVICE_CARD>(entity.DEVICE_ID);
                    //编码未发生修改则直接返回
                    if (curDevice != null && entity.TREE_NODE == curDevice.TREE_NODE) return;
                }
            }

            //使用广搜逐级更新子节点编码，同时判环
            var keySet = new HashSet<string>() { entity.DEVICE_ID };
            var list = new Queue<DEVICE_CARD>();
            list.Enqueue(entity);
            while (list.Any())
            {
                var parent = list.Dequeue();
                var childList = await _dbContext.Query<DEVICE_CARD>(x => x.PRE_DEVICE_ID == parent.DEVICE_ID)
                    .OrderBy(x => x.DEVICE_ID)
                    .ToListAsync();
                for (var i = 0; i < childList.Count; ++i)
                {
                    var child = childList[i];
                    MessageException.ThrowIf(keySet.Contains(child.DEVICE_ID), "出现上级链路循环");
                    keySet.Add(child.DEVICE_ID);
                    _dbContext.TrackEntity(child);
                    child.TREE_NODE = $"{parent.TREE_NODE}{i + 1:D3}";
                    await _dbContext.UpdateAsync(child);
                    list.Enqueue(child);
                }
            }
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

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> DeviceListAllAsync(GridRequest request)
        {
            var list = await _dbContext.Query<DEVICE_CARD>()
                .LeftJoin<DEVICE_DOC>((a, b) => a.DEVICE_ID == b.DEVICE_ID)
                .LeftJoin<DEVICE_PARAM>((a, b, c) => a.DEVICE_ID == c.DEVICE_ID)
                .LeftJoin<DEVICE_REMOULD>((a, b, c, d) => a.DEVICE_ID == d.DEVICE_ID)
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
