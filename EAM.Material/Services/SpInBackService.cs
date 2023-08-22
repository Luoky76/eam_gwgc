using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Grid;
using Gksyb.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAM.Material.Interfaces;
using NPOI.SS.Formula.PTG;
using Gksyb.Core.Auth;
using static StackExchange.Redis.Role;

namespace EAM.Material.Services
{
    public class SpInBackService : ISpInBackService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;
        private string masterID = string.Empty, errMsg = string.Empty;

        public SpInBackService(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userSession = userSession;
        }

        public async Task<GridData> ListAsync(GridRequest request)
        {
            var query = await _dbContext.Query<SP_IN_BACK>().GetGridData(request);
            return query;
        }

        public async Task<AjaxResult> GetAsync(string ID)
        {
            var query = await _dbContext.Query<SP_IN_BACK>().Where(x => x.IN_BACK_ID == ID).ToListAsync();

            return AjaxResult.Success(query);
        }

        public async Task<AjaxResult> InListAsync()
        {
            var result = await _dbContext.JoinQuery<SP_INSTORE, SP_INSTORE_DET>((a, b) => new object[]
               {
                   JoinType.LeftJoin,a.IN_ID.Equals(b.IN_ID)
               })
               .Where((a,b) => a.AUDITING == "1")
               .Select((a, b) => new {
                   a.AUDITING,
                   a.IN_CODE,
                   a.IN_DATE,
                   a.ORDER_CODE,
                   a.PROVIDER_NAME,
                   a.INSTORE_MONEY,
                   a.PUR_USER,
                   a.USER_NAME,
                   a.CHK_USER,
                   a.DEPT_NAME,
                   a.MEMO,
                   a.IN_ID,
                   b.SP_CODE,
                   b.SP_NAME,
                   b.UNIT,
                   b.SP_SIZE,
                   b.PRODUCE,
                   b.COUNT,
                   b.DELIVERY_CODE,
                   b.STOCK_NAME,
                   b.PRICE,
                   b.MONEY,
                   b.APPLY_USER,
                   b.APPLY_NO,
                   b.INDET_ID
               })
               .ToListAsync();
            return AjaxResult.Success(result, "成功");
        }

        public async Task<GridData> DetListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<SP_INBACK_DET>().GetGridData(request);
            return list;
        }

        public async Task<AjaxResult> Save(SaveRequest<SP_IN_BACK> request, SaveRequest<SP_INBACK_DET> requestdet)
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
                         c.DELIVERY_CODE,
                         c.BACK_CODE,
                         c.BACK_DATE,
                         c.ORDER_CODE,
                         c.PROVIDER_NAME,
                         c.PUR_USER,
                         c.CHK_USER,
                         c.EDIT_USER,
                         c.SUM_MONEY,
                         c.MEMO,
                         c.IN_ID,
                         c.IN_BACK_ID,
                     },
                     c => a => a.IN_BACK_ID == c.IN_BACK_ID
                     , BeforeAdd, BeforeUpdate, BeforeDelete, false, null, null);

                mainSuccess = !execResult.IsError;
                if (mainSuccess)  //主表是否保存成功
                {
                    requestdet = requestdet ?? new SaveRequest<SP_INBACK_DET>();

                    execResult = await _dbContext.SaveEntityAnsyc(requestdet,
                         c => new
                         {
                             c.SP_CODE,
                             c.SP_NAME,
                             c.UNIT,
                             c.SP_SIZE,
                             c.PRODUCE,
                             c.COUNT,
                             c.STOCK_NAME,
                             c.PRICE,
                             c.MONEY,
                             c.APPLY_USER,
                             c.APPLY_NO,
                             c.MEMO,
                             c.INDET_ID
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
        private async Task BeforeAdd(SP_IN_BACK entity)
        {
            entity.IN_BACK_ID = GuidHelper.NewSnowflakeId().ToString();
            masterID = entity.IN_BACK_ID;
            entity.EDIT_USER = _userSession.RealName;

            string type = "RCH" + DateTime.Now.ToString("yyyyMM");
            string def = type + "0000";
            var model = await _dbContext.Query<SP_IN_BACK>(x => x.BACK_CODE.Contains(type)).Select(x => Sql.Max(x.BACK_CODE) ?? def).FirstOrDefaultAsync();
            var index = model.SubStr(9, 4).CastTo<int>() + 1;
            entity.BACK_CODE = type + index.ToString("D4");

            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(SP_IN_BACK request)
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
                    SP_STORE _STORE = new();

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

                    await _dbContext.InsertAsync(_STORE);
                    await _dbContext.UpdateAsync<SP_INSTORE_DET>(x => iten.INDET_ID.Contains(x.INDET_ID),
                    x => new SP_INSTORE_DET
                    {
                        STORE_ID = _STORE.STORE_ID,
                    });
                }
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeDelete(SP_IN_BACK request)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        private async Task BeforeAddDet(SP_INBACK_DET entity)
        {
            entity.INDET_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.IN_BACK_ID = masterID;

            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeUpdateDet(SP_INBACK_DET request)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeDeleteDet(SP_INBACK_DET request)
        {
            await Task.CompletedTask;
        }
    }
}
