using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Services
{
    public class SpInstoreService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;
        private string errMsg = string.Empty;

        public SpInstoreService(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userSession = userSession;
        }

        /// <summary>
        /// 获取列表    
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<SP_INSTORE>().LeftJoin<SP_RECEIVE>((a, b) => a.RECEIVE_ID == b.RECEIVE_ID).Select((a, b) => new
            {
                a.AUDITING,
                a.IN_CODE,
                a.IN_DATE,
                b.ORDER_CODE,
                a.PROVIDER_NAME,
                a.INSTORE_MONEY,
                a.PUR_USER,
                b.CHK_DATE,
                a.USER_NAME,
                a.CHK_USER,
                a.DEPT_NAME,
                a.MEMO,
                a.IN_ID
            }).GetGridData(request);
            return list;
        }

        public async Task<AjaxResult> GetAsync(string ID)
        {
            var query = await _dbContext.Query<SP_INSTORE>().LeftJoin<SP_RECEIVE>((a, b) => a.RECEIVE_ID == b.RECEIVE_ID).Select((a, b) => new
            {
                a.AUDITING,
                a.IN_CODE,
                a.IN_DATE,
                b.ORDER_CODE,
                a.PROVIDER_NAME,
                a.INSTORE_MONEY,
                a.PUR_USER,
                b.CHK_DATE,
                a.USER_NAME,
                a.CHK_USER,
                a.DEPT_NAME,
                a.MEMO,
                a.IN_ID
            }).Where(c => c.IN_ID == ID).ToListAsync();

            return AjaxResult.Success(query);
        }

        /// <summary>
        /// 获取货位列表
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> HouseList()
        {
            var list = await _dbContext.Query<SP_HOUSE>(a => a.AUDITING == "1")
                .Select(c => new { STOCK_ID = c.HOUSE_ID, STOCK_NAME = c.HOUSE_NAME, STOCK_CODE = c.HOUSE_CODE })
                .ToListAsync();
            return AjaxResult.Success(list);
        }

        /// <summary>
        /// 获取明细列表    
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> DetListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<SP_INSTORE_DET>(a => a.IS_STOP == "0").GetGridData(request);
            return list;
        }

        public async Task<GridData> DetailListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<SP_INSTORE_DET>()
                .LeftJoin<SP_INSTORE>((a, b) => a.IN_ID == b.IN_ID)
                .LeftJoin<BASE_SPCATALOG>((a, b, c) => a.SP_CODE == c.SP_CODE)
                .LeftJoin<SP_STORE>((a, b, c, d) => a.STORE_ID == d.STORE_ID)
                .Where((a, b, c, d) => a.IS_STOP == "0" && b.AUDITING == "1")
                .Select((a, b, c, d) => new
                {
                    a.DELIVERY_CODE,
                    b.IN_CODE,
                    b.IN_DATE,
                    b.ORDER_CODE,
                    d.STORE_CODE,
                    b.PROVIDER_NAME,
                    b.PUR_USER,
                    a.SP_CODE,
                    b.USER_NAME,
                    b.CHK_USER,
                    a.DEPT_NAME,
                    a.MEMO,
                    a.SP_NAME,
                    a.SP_SIZE,
                    a.PRODUCE,
                    c.LAST_PROVIDER,
                    a.UNIT,
                    a.COUNT,
                    a.PRICE,
                    a.MONEY,
                    a.STOCK_CODE,
                    a.STOCK_NAME,
                    a.APPLY_USER,
                    a.APPLY_NO,
                    a.APPLY_MEMO,
                    a.USE_MEMO,
                    a.TAX_RATE,
                    a.NOTAX_PRICE,
                    a.UNTAX_MONEY,
                    c.LAST_PRICE,
                    a.INDET_ID,
                    a.IN_ID
                }).GetGridData(request);

            return list;
        }

        /// <summary>
        /// 同时保存主子表
        /// </summary>
        public async Task<AjaxResult> SaveAllAsync(SaveRequest<SP_INSTORE> request, SaveRequest<SP_INSTORE_DET> requestdet)
        {
            using (var trans = _dbContext.BeginTransaction())  //事务保证保存数据的一致性
            {
                bool mainSuccess = false, detSuccess = false;
                var execResult = await _dbContext.SaveEntityAnsyc(request,
                     c => new
                     {
                         c.AUDITING,
                         c.IN_CODE,
                         c.IN_DATE,
                         c.ORDER_CODE,
                         c.PROVIDER_NAME,
                         c.INSTORE_MONEY,
                         c.PUR_USER,
                         c.USER_NAME,
                         c.CHK_USER,
                         c.DEPT_NAME,
                         c.MEMO,
                         c.IN_ID
                     },
                     c => a => a.IN_ID == c.IN_ID
                     , BeforeAdd, BeforeUpdate, BeforeDelete, false, null, null);

                mainSuccess = !execResult.IsError;
                if (mainSuccess)  //主表是否保存成功
                {
                    requestdet = requestdet ?? new SaveRequest<SP_INSTORE_DET>();

                    execResult = await _dbContext.SaveEntityAnsyc(requestdet,
                         c => new
                         {
                             c.DELIVERY_CODE,
                             c.STOCK_NAME,
                             c.STOCK_ID,
                             c.STOCK_CODE,
                             c.INDET_ID,
                         },
                         c => a => a.INDET_ID == c.INDET_ID,
                         BeforeAddDet, BeforeUpdateDet, null, false, null, null);

                    detSuccess = !execResult.IsError;  //明细表是否保存成功
                }
                if (mainSuccess && detSuccess)
                    trans.Commit();
                else
                {
                    trans.Rollback();
                    if (string.IsNullOrWhiteSpace(errMsg)) errMsg = "保存失败";
                    return AjaxResult.Error(errMsg);
                }
            }
            return AjaxResult.Success("保存成功");
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        private async Task BeforeAdd(SP_INSTORE entity)
        {
            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = DateTime.Now;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = DateTime.Now;
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(SP_INSTORE request)
        {
            if (request.AUDITING == "7")
            {
                await _dbContext.UpdateAsync<SP_INSTORE_DET>(x => request.IN_ID.Contains(x.IN_ID),
                    x => new SP_INSTORE_DET
                    {
                        IS_STOP = "1",
                    });
            }
            else if (request.AUDITING == "1")
            {
                var det = await _dbContext.Query<SP_INSTORE_DET>(x => x.IN_ID == request.IN_ID)
                    .LeftJoin<SP_APPLY>((a, b) => a.APPLY_NO == b.APPLY_NO)
                    .Select((a, b) => new
                    {
                        b.DEPT_NAME,
                        b.DEPT_ID,
                        b.DEPT_CODE,
                        a.SP_CODE,
                        a.PRICE,
                        a.SP_ID,
                        a.SP_NAME,
                        a.SP_SIZE,
                        a.APPLY_NO,
                        a.DELIVERY_CODE,
                        a.STOCK_NAME,
                        a.RECDET_ID,
                        a.STOCK_ID,
                        a.APPLY_USER,
                        a.COUNT,
                        a.PRODUCE,
                        a.UNIT,
                        a.TYPE_NAME,
                        a.TYPE_CODE,
                        a.TYPE_ID,
                        a.MONEY,
                        a.MEMO,
                        a.NOTAX_PRICE,
                        a.UNTAX_MONEY,
                        a.INDET_ID,
                    })
                    .ToListAsync();

                foreach (var iten in det)
                {
                    SP_STORE _STORE = new();//库存表

                    _STORE.SRC_TYPE = "2";
                    _STORE.IS_BACK = "0";
                    _STORE.SP_CODE = iten.SP_CODE;
                    _STORE.SP_ID = iten.SP_ID;
                    _STORE.SP_NAME = iten.SP_NAME;
                    _STORE.SP_SIZE = iten.SP_SIZE;
                    _STORE.STOCK_NAME = iten.STOCK_NAME;
                    _STORE.UNIT = iten.UNIT;
                    _STORE.PRODUCE = iten.PRODUCE;
                    _STORE.TYPE_NAME = iten.TYPE_NAME;
                    _STORE.TYPE_CODE = iten.TYPE_CODE;
                    _STORE.TYPE_ID = iten.TYPE_ID;
                    _STORE.NUM = iten.COUNT;
                    _STORE.PRICE = iten.PRICE;
                    _STORE.MONEY = iten.MONEY;
                    _STORE.NOTAX_PRICE = iten.NOTAX_PRICE;
                    _STORE.NOTAX_MONEY = iten.UNTAX_MONEY;
                    _STORE.PROVIDER_NAME = request.PROVIDER_NAME;
                    _STORE.APPLY_NO = iten.APPLY_NO;
                    _STORE.DELIVERY_CODE = iten.DELIVERY_CODE;
                    _STORE.INDET_ID = iten.INDET_ID;
                    _STORE.STORE_ID = GuidHelper.NewSnowflakeId().ToString();
                    _STORE.IN_CODE = request.IN_CODE;
                    _STORE.DEPT_ID = iten.DEPT_ID;
                    _STORE.DEPT_NAME = iten.DEPT_NAME;
                    _STORE.CREATE_USERID = _userSession.UserID.ToString();
                    _STORE.CREATEDATE = DateTime.Now;
                    _STORE.MODIFY_USERID = _userSession.UserID.ToString();
                    _STORE.MODIFYDATE = DateTime.Now;

                    string type = "PC" + DateTime.Now.ToString("yyyyMM");
                    string def = type + "0000";
                    var model = await _dbContext.Query<SP_STORE>(x => x.STORE_CODE.Contains(type)).Select(x => Sql.Max(x.STORE_CODE) ?? def).FirstOrDefaultAsync();
                    var index = model.SubStr(8, 4).CastTo<int>() + 1;
                    _STORE.STORE_CODE = type + index.ToString("D4");

                    STORE_WATER _WATER = new();//库存流水表

                    _WATER.WATER_ID = GuidHelper.NewSnowflakeId().ToString();
                    _WATER.SRC_TYPE = "2";
                    _WATER.IS_BACK = "0";
                    _WATER.STORE_ID = _STORE.STORE_ID;
                    _WATER.WATER_DATE = DateTime.Now;
                    _WATER.SRC_CODE = request.IN_CODE;
                    _WATER.SP_CODE = iten.SP_CODE;
                    _WATER.SP_NAME = iten.SP_NAME;
                    _WATER.SP_SIZE = iten.SP_SIZE;
                    _WATER.IN_NUM = iten.COUNT;
                    _WATER.IN_PRICE = iten.PRICE;
                    _WATER.IN_MONEY = iten.MONEY;
                    _WATER.CUR_NUM = iten.COUNT;
                    _WATER.CUR_MONEY = iten.MONEY;


                    await _dbContext.InsertAsync(_STORE);
                    await _dbContext.InsertAsync(_WATER);
                    await _dbContext.UpdateAsync<SP_INSTORE_DET>(x => iten.INDET_ID.Contains(x.INDET_ID),
                    x => new SP_INSTORE_DET
                    {
                        STORE_ID = _STORE.STORE_ID,
                    });
                }
            }
            request.MODIFY_USERID = _userSession.UserID.ToString();
            request.MODIFYDATE = DateTime.Now;
            await Task.CompletedTask;
        }

        public async Task<AjaxResult> SubmitAsync(List<string> sids)
        {
            if (sids == null || sids.Count == 0) return AjaxResult.Error("请选择行");

            using (var trans = _dbContext.BeginTransaction())
            {
                foreach (var sid in sids)
                {
                    var entity = await _dbContext.Query<SP_INSTORE>(x => x.IN_ID == sid).FirstOrDefaultAsync();
                    if (entity == null) continue;
                    if (entity.AUDITING == "1")
                    {
                        trans.Rollback();
                        return AjaxResult.Error("该数据已提交，无法重复提交！");
                    }
                    if (!entity.IN_DATE.HasValue)
                    {
                        trans.Rollback();
                        return AjaxResult.Error("入库日期未填写！");
                    }
                    if (entity.AUDITING == "7")
                    {
                        trans.Rollback();
                        return AjaxResult.Error("该数据已注销，无法提交！");
                    }

                    entity.AUDITING = "1";
                    await BeforeUpdate(entity);
                    await _dbContext.UpdateAsync(entity);
                }
                trans.Commit();
            }
            return AjaxResult.Success("提交成功");
        }

        public async Task<AjaxResult> BackAsync(List<string> sids)
        {
            if (sids == null || sids.Count == 0) return AjaxResult.Error("请选择行");

            using (var trans = _dbContext.BeginTransaction())
            {
                foreach (var sid in sids)
                {
                    var entity = await _dbContext.Query<SP_INSTORE>(x => x.IN_ID == sid).FirstOrDefaultAsync();
                    if (entity == null) continue;
                    if (entity.AUDITING == "1")
                    {
                        trans.Rollback();
                        return AjaxResult.Error("该数据已提交，无法退回验收！");
                    }
                    if (entity.AUDITING == "7")
                    {
                        trans.Rollback();
                        return AjaxResult.Error("该数据已注销，无法退回验收！");
                    }

                    entity.AUDITING = "7";
                    await BeforeUpdate(entity);
                    await _dbContext.UpdateAsync(entity);
                }
                trans.Commit();
            }
            return AjaxResult.Success("提交成功");
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeDelete(SP_INSTORE request)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        private async Task BeforeAddDet(SP_INSTORE_DET entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeUpdateDet(SP_INSTORE_DET request)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeDeleteDet(SP_INSTORE_DET request)
        {
            await Task.CompletedTask;
        }
    }
}
