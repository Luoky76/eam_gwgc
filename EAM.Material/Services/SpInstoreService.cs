using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Core.Interfaces.General;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Services
{
    public class SpInstoreService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly ICodeCreatorService _codeCreatorService;
        private readonly IComboxDataService _comboxDataService;
        private readonly ICorpService _corpService;
        private readonly UserSession _userSession;
        public SpInstoreService(IDbContext dbContext, ICodeCreatorService codeCreatorService, IComboxDataService comboxDataService, ICorpService corpService, UserSession userSession)
        {
            _dbContext = dbContext;
            _codeCreatorService = codeCreatorService;
            _comboxDataService = comboxDataService;
            _corpService = corpService;
            _userSession = userSession;
        }

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public async Task<AjaxResult> ComboxDataAsync()
        {
            try
            {
                var data = await _comboxDataService.Get(new Dictionary<string, object>()
                {
                    { "BCCode@#Auditing", "auditing" }
                });
                return AjaxResult.Success(data);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<SP_INSTORE>().GetGridData(request);
            return list;
        }

        /// <summary>
        /// 获取记录
        /// </summary>
        public async Task<AjaxResult> GetAsync(string inId)
        {
            var query = await _dbContext.Query<SP_INSTORE>()
                .Where(c => c.IN_ID == inId)
                .FirstOrDefaultAsync();

            return AjaxResult.Success(query);
        }

        /// <summary>
        /// 获取货位列表
        /// </summary>
        public async Task<AjaxResult> HouseList()
        {
            var list = await _dbContext.Query<SP_HOUSE>(a => a.AUDITING == "1")
                .Select(c => new { HOUSE_ID = c.HOUSE_ID, HOUSE_NAME = c.HOUSE_NAME, HOUSE_CODE = c.HOUSE_CODE })
                .ToListAsync();
            return AjaxResult.Success(list);
        }

        /// <summary>
        /// 获取明细列表
        /// </summary>
        public async Task<GridData> DetListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<SP_INSTORE_DET>().GetGridData(request);
            return list;
        }

        /// <summary>
        /// 获取明细列表（详情汇总用）
        /// </summary>
        public async Task<GridData> DetailListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<SP_INSTORE_DET>()
                .LeftJoin<SP_INSTORE>((a, b) => a.IN_ID == b.IN_ID)
                .LeftJoin<BASE_SPCATALOG>((a, b, c) => a.SP_CODE == c.SP_CODE)
                .Where((a, b, c) => b.AUDITING == "1")
                .Select((a, b, c) => new
                {
                    b.IN_CODE,
                    b.IN_DATE,
                    b.PROVIDER_NAME,
                    b.IN_USER,
                    b.CHK_USER,
                    b.DEPT_NAME,
                    b.MEMO,
                    a.SP_CODE,
                    a.SP_NAME,
                    a.SP_SIZE,
                    a.PRODUCE,
                    a.UNIT,
                    a.IN_NUM,
                    a.TAX_PRICE,
                    a.TAX_MONEY,
                    a.HOUSE_NAME,
                    a.TAX_RATE,
                    a.NOTAX_PRICE,
                    a.NOTAX_MONEY,
                    c.LAST_PROVIDER,
                    c.LAST_PRICE,
                    a.IN_DET_ID,
                    a.IN_ID
                }).GetGridData(request);

            return list;
        }

        /// <summary>
        /// 同时保存主子表
        /// </summary>
        public async Task<AjaxResult> SaveAllAsync(SaveRequest<SP_INSTORE> request, SaveRequest<SP_INSTORE_DET> requestdet)
        {
            await _dbContext.UseTransactionAsync(async () =>
            {
                bool mainSuccess = false, detSuccess = false;
                var execResult = await _dbContext.SaveEntityAnsyc(request,
                     c => new
                     {
                         c.AUDITING,
                         c.IN_CODE,
                         c.IN_DATE,
                         c.PROVIDER_NAME,
                         c.IN_USERID,
                         c.IN_USER,
                         c.CHK_USERID,
                         c.CHK_USER,
                         c.DEPT_ID,
                         c.DEPT_NAME,
                         c.MEMO,
                         c.CONSIGNEE_ID,
                         c.CONSIGNEE,
                         c.IN_ID
                     },
                     c => a => a.IN_ID == c.IN_ID
                     , BeforeAdd, BeforeUpdate, BeforeDelete, orgin: true);

                mainSuccess = !execResult.IsError;
                if (mainSuccess)  //主表是否保存成功
                {
                    requestdet = requestdet ?? new SaveRequest<SP_INSTORE_DET>();

                    execResult = await _dbContext.SaveEntityAnsyc(requestdet,
                         c => new
                         {
                             c.IN_ID,
                             c.COLLECT_REQUEST_ID,
                             c.REQUEST_DET_ID,
                             c.SP_ID,
                             c.SP_CODE,
                             c.SP_NAME,
                             c.SP_SIZE,
                             c.PRODUCE,
                             c.UNIT,
                             c.TYPE_ID,
                             c.TYPE_NAME,
                             c.HOUSE_ID,
                             c.HOUSE_NAME,
                             c.HOUSE_CODE,
                             c.IN_NUM,
                             c.TAX_RATE,
                             c.TAX_PRICE,
                             c.TAX_MONEY,
                             c.NOTAX_PRICE,
                             c.NOTAX_MONEY,
                             c.IN_DET_ID,
                         },
                         c => a => a.IN_DET_ID == c.IN_DET_ID,
                         BeforeAddDet, BeforeUpdateDet, null, orgin: true);

                    detSuccess = !execResult.IsError;  //明细表是否保存成功
                }
                if (!mainSuccess || !detSuccess)
                {
                    throw new MessageException("保存失败");
                }
            });
            return AjaxResult.Success("保存成功");
        }

        /// <summary>
        /// 新增前
        /// </summary>
        private async Task BeforeAdd(SP_INSTORE entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前（提交时触发）
        /// </summary>
        private async Task BeforeUpdate(SP_INSTORE request)
        {
            if (request.AUDITING == "1")
            {
                var det = await _dbContext.Query<SP_INSTORE_DET>(x => x.IN_ID == request.IN_ID).ToListAsync();

                foreach (var item in det)
                {
                    //生成库存记录
                    SP_STORE _STORE = new();
                    _STORE.SRC_TYPE = "2";
                    _STORE.IS_BACK = "0";
                    _STORE.SP_CODE = item.SP_CODE;
                    _STORE.SP_ID = item.SP_ID;
                    _STORE.SP_NAME = item.SP_NAME;
                    _STORE.SP_SIZE = item.SP_SIZE;
                    _STORE.UNIT = item.UNIT;
                    _STORE.PRODUCE = item.PRODUCE;
                    _STORE.TYPE_NAME = item.TYPE_NAME;
                    _STORE.TYPE_ID = item.TYPE_ID;
                    _STORE.NUM = item.IN_NUM;
                    _STORE.PRICE = item.TAX_PRICE;
                    _STORE.MONEY = item.TAX_MONEY;
                    _STORE.NOTAX_PRICE = item.NOTAX_PRICE;
                    _STORE.NOTAX_MONEY = item.NOTAX_MONEY;
                    _STORE.PROVIDER_NAME = request.PROVIDER_NAME;
                    _STORE.INDET_ID = item.IN_DET_ID;
                    _STORE.STORE_ID = GuidHelper.NewSnowflakeId().ToString();
                    _STORE.IN_CODE = request.IN_CODE;
                    _STORE.DEPT_ID = request.DEPT_ID;
                    _STORE.DEPT_NAME = request.DEPT_NAME;

                    if (_STORE.STORE_CODE.IsNullOrWhiteSpace())
                    {
                        _STORE.STORE_CODE = await _codeCreatorService.CreateCodeAsync<SP_STORE>("PC", a => a.STORE_CODE);
                    }

                    //生成库存流水记录
                    STORE_WATER _WATER = new();
                    _WATER.WATER_ID = GuidHelper.NewSnowflakeId().ToString();
                    _WATER.SRC_TYPE = "2";
                    _WATER.IS_BACK = "0";
                    _WATER.STORE_ID = _STORE.STORE_ID;
                    _WATER.WATER_DATE = DateTime.Now;
                    _WATER.SRC_CODE = request.IN_CODE;
                    _WATER.SP_CODE = item.SP_CODE;
                    _WATER.SP_NAME = item.SP_NAME;
                    _WATER.SP_SIZE = item.SP_SIZE;
                    _WATER.IN_NUM = item.IN_NUM;
                    _WATER.IN_PRICE = item.TAX_PRICE;
                    _WATER.IN_MONEY = item.TAX_MONEY;
                    _WATER.CUR_NUM = item.IN_NUM;
                    _WATER.CUR_MONEY = item.TAX_MONEY;

                    await _dbContext.InsertAsync(_STORE);
                    await _dbContext.InsertAsync(_WATER);
                }
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 提交
        /// </summary>
        public async Task<AjaxResult> SubmitAsync(List<string> sids)
        {
            if (sids == null || sids.Count == 0) return AjaxResult.Error("请选择行");

            await _dbContext.UseTransactionAsync(async () =>
            {
                foreach (var sid in sids)
                {
                    var entity = await _dbContext.Query<SP_INSTORE>(x => x.IN_ID == sid).FirstOrDefaultAsync();
                    if (entity == null) continue;
                    if (entity.AUDITING == "1")
                    {
                        throw new Exception("该数据已提交，无法重复提交！");
                    }
                    if (!entity.IN_DATE.HasValue)
                    {
                        throw new Exception("入库日期未填写！");
                    }

                    entity.AUDITING = "1";
                    await BeforeUpdate(entity);
                    await _dbContext.UpdateAsync(entity);
                }
            });
            return AjaxResult.Success("提交成功");
        }

        /// <summary>
        /// 退回
        /// </summary>
        public async Task<AjaxResult> BackAsync(List<string> sids)
        {
            if (sids == null || sids.Count == 0) return AjaxResult.Error("请选择行");

            await _dbContext.UseTransactionAsync(async () =>
            {
                foreach (var sid in sids)
                {
                    var entity = await _dbContext.Query<SP_INSTORE>(x => x.IN_ID == sid).FirstOrDefaultAsync();
                    if (entity == null) continue;
                    if (entity.AUDITING == "1")
                    {
                        throw new Exception("该数据已提交，无法退回验收！");
                    }

                    entity.AUDITING = "7";
                    await _dbContext.UpdateAsync(entity);
                }
            });
            return AjaxResult.Success("提交成功");
        }

        /// <summary>
        /// 删除前
        /// </summary>
        private async Task BeforeDelete(SP_INSTORE request)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 新增明细前
        /// </summary>
        private async Task BeforeAddDet(SP_INSTORE_DET entity)
        {
            if (entity.IN_DET_ID.IsNullOrWhiteSpace())
            {
                entity.IN_DET_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新明细前
        /// </summary>
        private async Task BeforeUpdateDet(SP_INSTORE_DET request)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除明细前
        /// </summary>
        private async Task BeforeDeleteDet(SP_INSTORE_DET request)
        {
            await Task.CompletedTask;
        }
    }
}
