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
using Gksyb.Core.Auth;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace EAM.Material.Services
{
    public class SpReceiveService : ISpReceiveService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;
        private string masterID = string.Empty, errMsg = string.Empty;
        private string masterInID = string.Empty;

        public SpReceiveService(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userSession = userSession;
        }
        /// <summary>
        /// 获取下拉框信息
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ComboxData()
        {
            try
            {
                var dic = await _comboxDataService.Get(new Dictionary<string, object>()
                {
                    { "SpHouseName", (Expression<Func<SP_HOUSE, bool>>)null}
                });
                return AjaxResult.Success(dic);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }

        public async Task<GridData> ListAsync(GridRequest request)
        {
            var query = await _dbContext.Query<SP_RECEIVE>().GetGridData(request);
            return query;
        }

        public async Task<AjaxResult> GetAsync(string ID)
        {
            var query = await _dbContext.Query<SP_RECEIVE>().Where(x => x.RECEIVE_ID == ID).ToListAsync();

            return AjaxResult.Success(query);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> Save(SaveRequest<SP_RECEIVE> request, SaveRequest<SP_RECEIVE_DET> requestdet)
        {
            using (var trans = _dbContext.BeginTransaction())  //事务保证保存数据的一致性
            {
                bool mainSuccess = false, detSuccess = false;
                request = request ?? new SaveRequest<SP_RECEIVE>();
                var execResult = await _dbContext.SaveEntityAnsyc(request,
                     c => new
                     {
                         c.AUDITING,
                         c.AUDITING_CHK,
                         c.RECEIVE_CODE,
                         c.RECEIVE_DATE,
                         c.USER_NAME,
                         c.PROVIDER_NAME,
                         c.ORDER_CODE,
                         c.ORDER_ID,
                         c.DEPT_NAME,
                         c.PUR_USER,
                         c.CHK_USER,
                         c.REG_MEMO,
                         c.MEMO,
                         c.RECEIVE_ID
                     },
                     c => a => a.RECEIVE_ID == c.RECEIVE_ID
                     , BeforeAdd, BeforeUpdate, BeforeDelete, false, null, null);

                mainSuccess = !execResult.IsError;
                if (mainSuccess)  //主表是否保存成功
                {
                    requestdet = requestdet ?? new SaveRequest<SP_RECEIVE_DET>();

                    execResult = await _dbContext.SaveEntityAnsyc(requestdet,
                         c => new
                         {
                             c.SP_CODE,
                             c.SP_NAME,
                             c.UNIT,
                             c.SP_SIZE,
                             c.PRODUCE,
                             c.MEMO,
                             c.COUNT,
                             c.CHK_COUNT,
                             c.CHK_MONEY,
                             c.RETURN_MEMO,
                             c.DELIVERY_CODE,
                             c.STOCK_NAME,
                             c.PRICE,
                             c.MONEY,
                             c.APPLY_USER,
                             c.APPLY_NO,
                             c.RECDET_ID,
                             c.ORDERDET_ID,
                             c.RECEIVE_ID
                         },
                         c => a => a.RECDET_ID == c.RECDET_ID
                         , DetBeforAdd, DetBeforUpdate, null, false, null, null);

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
        private async Task BeforeAdd(SP_RECEIVE entity)
        {
            entity.RECEIVE_ID = masterID = GuidHelper.NewSnowflakeId().ToString();
            entity.USER_NAME = _userSession.UserName.ToString();

            string type = "DJ" + DateTime.Now.ToString("yyyyMM");
            string def = type + "0000";
            var model = await _dbContext.Query<SP_RECEIVE>(x => x.RECEIVE_CODE.Contains(type)).Select(x => Sql.Max(x.RECEIVE_CODE) ?? def).FirstOrDefaultAsync();
            var index = model.SubStr(8, 4).CastTo<int>() + 1;
            entity.RECEIVE_CODE = type + index.ToString("D4");

            

            await Task.CompletedTask;
        }

        private async Task DetBeforAdd(SP_RECEIVE_DET entity)
        {
            if (string.IsNullOrWhiteSpace(entity.COUNT.ToString()) || string.IsNullOrWhiteSpace(entity.STOCK_NAME) || string.IsNullOrWhiteSpace(entity.DELIVERY_CODE))
            {
                errMsg = "数量，送货单号，收货库位为必填项！";
                throw new MessageException(errMsg);
            }
            entity.RECEIVE_ID = entity.RECEIVE_ID ?? masterID; 
            entity.RECDET_ID = GuidHelper.NewSnowflakeId().ToString();

            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(SP_RECEIVE entity)
        {
            if (entity.AUDITING.Equals("0"))
            {
                var sysDate = await _dbContext.GetSysdate();
                masterID = entity.RECEIVE_ID;
                entity.MODIFY_USERID = _userSession.UserID.ToString();
                entity.MODIFYDATE = sysDate;
            }
            if (entity.AUDITING_CHK == "1")
            {
                entity.CHK_DATE = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                entity.EDIT_USER = _userSession.UserName;

                SP_INSTORE _in = new();

                _in.AUDITING = "0";

                string type = "RK" + DateTime.Now.ToString("yyyyMM");
                string def = type + "0000";
                var model = await _dbContext.Query<SP_INSTORE>(x => x.IN_CODE.Contains(type)).Select(x => Sql.Max(x.IN_CODE) ?? def).FirstOrDefaultAsync();
                var index = model.SubStr(8, 4).CastTo<int>() + 1;
                _in.IN_CODE = type + index.ToString("D4");

                _in.ORDER_CODE = entity.ORDER_CODE;
                _in.PROVIDER_NAME = entity.PROVIDER_NAME;
                //_in.INSTORE_MONEY = entity.INSTORE_MONEY;
                _in.PUR_USER = entity.PUR_USER;
                _in.USER_NAME = entity.USER_NAME;
                _in.CHK_USER = entity.EDIT_USER;
                _in.DEPT_NAME = entity.DEPT_NAME;
                _in.IN_ID = GuidHelper.NewSnowflakeId().ToString();
                masterInID = _in.IN_ID;
                _in.RECEIVE_ID = entity.RECEIVE_ID;

                decimal? money = 0;

                var recdet = await _dbContext.Query<SP_RECEIVE_DET>(x => x.RECEIVE_ID == entity.RECEIVE_ID).ToListAsync();
                if (recdet.Count() > 0)
                {
                    foreach (var item in recdet)
                    {
                        SP_INSTORE_DET _indet = new();

                        _indet.SP_CODE = item.SP_CODE;
                        _indet.SP_NAME = item.SP_NAME;
                        _indet.SP_SIZE = item.SP_SIZE;
                        _indet.PRODUCE = item.PRODUCE;
                        _indet.UNIT = item.UNIT;
                        _indet.COUNT = item.COUNT;
                        _indet.PRICE = item.PRICE;
                        _indet.MONEY = item.CHK_MONEY;
                        _indet.TAX_RATE = item.TAX_RATE;
                        _indet.NOTAX_PRICE = item.NOTAX_PRICE;
                        _indet.UNTAX_MONEY = item.UNTAX_MONEY;
                        _indet.DELIVERY_CODE = item.DELIVERY_CODE;
                        _indet.MEMO = item.STOCK_NAME;
                        _indet.APPLY_USER = item.APPLY_USER;
                        _indet.DEPT_NAME = item.DEPT_NAME;
                        _indet.APPLY_NO = item.APPLY_NO;
                        _indet.IN_ID = masterInID;
                        _indet.INDET_ID = GuidHelper.NewSnowflakeId().ToString();
                        _indet.RECDET_ID = item.RECDET_ID;
                        money += item.CHK_MONEY;

                        await _dbContext.InsertAsync<SP_INSTORE_DET>(_indet);
                    }

                    //采购申请进度更新
                    var appledetId = recdet.Select(t => t.SPDET_ID).ToList();
                    await _dbContext.UpdateAsync<SP_APPLY_DETAIL>(x => appledetId.Contains(x.SPDET_ID),
                     x => new SP_APPLY_DETAIL
                     {
                         SP_STATUS = "60"//供货中
                     });
                }

                _in.INSTORE_MONEY = money;

                await _dbContext.InsertAsync<SP_INSTORE>(_in);
            }
           
            if (entity.AUDITING_CHK == "-1") //撤销提交
            {
                entity.AUDITING_CHK = "0";
                entity.CHK_DATE = null;
                entity.EDIT_USER = _userSession.UserName;

                if (_dbContext.Query<SP_INSTORE>().Any(t => t.RECEIVE_ID == entity.RECEIVE_ID && t.AUDITING != "0"))
                {
                    errMsg = $"{entity.RECEIVE_CODE}已采购入库,不能撤销!";
                    throw new MessageException(errMsg);
                }
                await _dbContext.DeleteAsync<SP_INSTORE>(x => x.RECEIVE_ID.Equals(entity.RECEIVE_ID));
                var recdet = await _dbContext.Query<SP_RECEIVE_DET>(x => x.RECEIVE_ID == entity.RECEIVE_ID).ToListAsync();
                //采购申请进度更新
                var appledetId = recdet.Select(t => t.SPDET_ID).ToList();
                await _dbContext.UpdateAsync<SP_APPLY_DETAIL>(x => appledetId.Contains(x.SPDET_ID),
                 x => new SP_APPLY_DETAIL
                 {
                     SP_STATUS = "50"//供货中
                 });
                var Ids = recdet.Select(t => t.RECDET_ID).ToList();
                await _dbContext.DeleteAsync<SP_INSTORE_DET>(x => Ids.Contains(x.RECDET_ID));

            }
            await Task.CompletedTask;
        }

        private async Task DetBeforUpdate(SP_RECEIVE_DET entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(SP_RECEIVE entity)
        {
            await _dbContext.DeleteAsync<SP_RECEIVE_DET>(x => x.RECEIVE_ID == entity.RECEIVE_ID);

            await Task.CompletedTask;
        }

        /// <summary>
        /// 采购订单列表
        /// </summary>
        /// <returns></returns>
        public AjaxResult OrderList()
        {
            try
            {
                var result = _dbContext.JoinQuery<SP_ORDER, SP_ORDER_DETAIL, SP_RECEIVE_DET, SP_RECEIVE>((a, b, c, d) => new object[]
                    {
                    JoinType.LeftJoin, a.ORDER_ID.Equals(b.ORDER_ID),
                    JoinType.LeftJoin, b.ORDERDET_ID.Equals(c.ORDERDET_ID),
                    JoinType.LeftJoin, d.RECEIVE_ID.Equals(c.RECEIVE_ID)
                    })
                    .Where((a, b, c, d)=>a.AUDITING == "1" )
                   .Select((a, b, c, d) => new
                   {
                       Order = a,
                       OrderDet = b,
                       a.ORDER_ID,
                       a.ORDER_CODE,
                       c.ORDERDET_ID,
                       detcount = c.COUNT,
                       actcount = b.COUNT - b.STOP_NUM,
                   })
                   .GroupBy(c => new
                   {
                       c.ORDER_ID,
                       c.ORDER_CODE,
                       c.ORDERDET_ID,
                       c.detcount,
                       c.actcount,
                   })
                   .Select(c => new
                   {
                       c.ORDER_ID,
                       c.ORDER_CODE,
                       c.ORDERDET_ID,
                       c.actcount,
                       sumdetcount = Sql.Sum(c.detcount)?? 0,
                       ORDER_DATE = c.Order.ORDER_DATE,
                       BUY_USER = c.Order.BUY_USER?? "",
                       ORDER_MONEY = c.Order.ORDER_MONEY?? 0,
                       PROVIDER_NAME = c.Order.PROVIDER_NAME?? "",
                       DEPT_NAME = c.Order.DEPT_NAME?? "",
                   })
                   .Where(c => (c.actcount -c.sumdetcount)>0)
                   .ToList()
                   .DistinctBy(c => c.ORDER_CODE);

                return AjaxResult.Success(result, "成功");
            }
            catch (Exception ex)
            {
                throw new MessageException(ex.Message);
            }
        }

        /// <summary>
        /// 物料
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> SpList(GridRequest request)
        {
            try
            {
                return await _dbContext.JoinQuery<SP_ORDER_DETAIL,  SP_RECEIVE_DET, SP_RECEIVE, SP_ORDER>((a, b, c, d) => new object[]
                    {
                    JoinType.LeftJoin, a.ORDERDET_ID.Equals(b.ORDERDET_ID),
                    JoinType.LeftJoin, c.RECEIVE_ID.Equals(b.RECEIVE_ID),
                    JoinType.LeftJoin, a.ORDER_ID.Equals(d.ORDER_ID),
                    })
                    .Where((a, b, c, d) => c.AUDITING == "1")
                   .Select((a, b, c, d) => new
                   {
                       OrderDet = a,
                       a.ORDERDET_ID,
                       detcount = b.COUNT,
                       actcount = a.COUNT - a.STOP_NUM,
                   })
                   .GroupBy(c => new
                   {
                       c.ORDERDET_ID,
                       c.detcount,
                       c.actcount,
                   })
                   .Select(c => new
                   {
                       c.ORDERDET_ID,
                       c.actcount,
                       sumdetcount = Sql.Sum(c.detcount)?? 0,
                       c.OrderDet.SP_CODE,
                       c.OrderDet.SP_NAME,
                       c.OrderDet.SP_SIZE,
                       c.OrderDet.APPLY_NO,
                       c.OrderDet.APPLY_USER,
                       c.OrderDet.DEPT_NAME,
                       c.OrderDet.PRODUCE,
                       c.OrderDet.UNIT,
                       c.OrderDet.TYPE_NAME,
                       c.OrderDet.COUNT,
                       c.OrderDet.STOP_NUM,
                       c.OrderDet.PRICE,
                       c.OrderDet.MONEY,
                       c.OrderDet.REQ_DATE,
                       c.OrderDet.MEMO,
                       c.OrderDet.ORDER_ID,
                   })
                   .Where(c => (c.actcount-c.sumdetcount)>0)
                   .GetGridData(request);

            }
            catch (Exception ex)
            {
                throw new MessageException(ex.Message);
            }
        }

        /// <summary>
        /// 订单物料列表
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ApplyList()
        {
            var result = await _dbContext.JoinQuery<SP_APPLY, SP_APPLY_DETAIL>((a, b) => new object[]
               {
                   JoinType.LeftJoin,a.APPLY_ID.Equals(b.APPLY_ID)
               })
               .Select((a, b) => new { a.APPLY_ID, b.SP_CODE, b.SP_NAME })
               .ToListAsync();
            return AjaxResult.Success(result, "成功");
        }

        public async Task<GridData> DetListAsync(GridRequest request)
        {
            try
            {
                return await _dbContext.JoinQuery<SP_RECEIVE_DET, SP_ORDER_DETAIL>((a, b) => new object[]
                    {
                    JoinType.LeftJoin, a.ORDERDET_ID.Equals(b.ORDERDET_ID),
                    })
                   .Select((a, b) => new
                   {
                       a.ORDERDET_ID,
                       DDCOUNT = b.COUNT,
                       ZZCOUNT = b.STOP_NUM,
                       a.COUNT,
                       a.SP_CODE,
                       a.SP_NAME,
                       a.SP_SIZE,
                       a.APPLY_NO,
                       a.APPLY_USER,
                       a.DEPT_NAME,
                       a.PRODUCE,
                       a.UNIT,
                       a.TYPE_NAME,
                       a.PRICE,
                       a.MONEY,
                       a.MEMO,
                       a.RECEIVE_ID,
                   })
                   .GetGridData(request);

            }
            catch (Exception ex)
            {
                throw new MessageException(ex.Message);
            }
            /*var query = await _dbContext.Query<SP_RECEIVE_DET>().GetGridData(request);

            return query;*/
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> SaveDet(SaveRequest<SP_RECEIVE_DET> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                a => new
                {
                    a.SP_CODE,
                    a.SP_NAME,
                    a.SP_SIZE,
                    a.PRODUCE,
                    a.UNIT,
                    a.COUNT,
                    a.PRICE,
                    a.MONEY,
                    a.APPLY_USER,
                    a.MEMO,
                    a.DELIVERY_CODE,
                    a.RECDET_ID,
                },
                c => a => a.RECDET_ID == c.RECDET_ID, BeforeAddItem, BeforeUpdateItem, BeforeDeleteItem, false);
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        private async Task BeforeAddItem(SP_RECEIVE_DET entity)
        {
            entity.RECDET_ID = GuidHelper.NewSnowflakeId().ToString();

            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeUpdateItem(SP_RECEIVE_DET request)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns> 
        private async Task BeforeDeleteItem(SP_RECEIVE_DET request)
        {
            await Task.CompletedTask;
        }
    }
}
