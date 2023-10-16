using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Grid;
using Gksyb.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gksyb.Core.Grid;
using EAM.Material.Interfaces;
using NPOI.SS.Formula.PTG;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Office.CustomUI;

namespace EAM.Material.Services
{
    public class SpInstoreService : ISpInstoreService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private string errMsg = string.Empty;

        public SpInstoreService(IDbContext dbContext, IComboxDataService comboxDataService)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
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
                .Select(c => new { STOCK_ID = c.HOUSE_ID, STOCK_NAME = c.HOUSE_NAME,STOCK_CODE = c.HOUSE_CODE })
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
            var list = await _dbContext.Query<SP_INSTORE_DET>(a=>a.IS_STOP=="0").GetGridData(request);
            return list;
        }

        public async Task<GridData> DetailListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<SP_INSTORE_DET>()
                .LeftJoin<SP_INSTORE>((a, b) => a.IN_ID == b.IN_ID)
                .LeftJoin<BASE_SPCATALOG>((a,b,c) => a.SP_CODE == c.SP_CODE)
                .LeftJoin<SP_STORE>((a,b,c,d) => a.STORE_ID == d.STORE_ID)
                .Where((a, b, c, d)=> a.IS_STOP=="0"&& b.AUDITING =="1")
                .Select((a, b,c,d) => new
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
        /// 保存
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> Save(SaveRequest<SP_INSTORE> request, SaveRequest<SP_INSTORE_DET> requestdet)
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
                var det = await _dbContext.Query<SP_INSTORE_DET>(x => x.IN_ID == request.IN_ID).ToListAsync();

                foreach (var iten in det)
                {
                    SP_STORE _STORE = new();//库存表

                    _STORE.SRC_TYPE = "2";
                    _STORE.IS_BACK = "0";
                    _STORE.SP_CODE = iten.SP_CODE;
                    _STORE.SP_NAME = iten.SP_NAME;
                    _STORE.SP_SIZE = iten.SP_SIZE;
                    _STORE.STOCK_NAME = iten.STOCK_NAME;
                    _STORE.UNIT = iten.UNIT;
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
            else if (request.AUDITING == "-1") //撤销提交
            {
                request.AUDITING = "0";
                var detIds = await _dbContext.Query<SP_INSTORE_DET>(x => x.IN_ID == request.IN_ID).Select(t=>t.INDET_ID).ToListAsync();

                await _dbContext.UpdateAsync<SP_INSTORE_DET>(x => detIds.Contains(x.INDET_ID),
                    x => new SP_INSTORE_DET
                    {
                        STORE_ID = null,
                    });

                var store = await _dbContext.Query<SP_STORE>(x => detIds.Contains(x.INDET_ID)).ToListAsync();
                var storeId = store.Select(t => t.STORE_ID).ToList();
                await _dbContext.DeleteAsync<SP_STORE>(x => storeId.Contains(x.STORE_ID));
                await _dbContext.DeleteAsync<STORE_WATER>(x => storeId.Contains(x.STORE_ID));
            }
            await Task.CompletedTask;
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
