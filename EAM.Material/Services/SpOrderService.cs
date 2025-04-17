using EAM.Material.DTO;
using EAM.Material.Interfaces;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using System.Linq.Expressions;

namespace EAM.Material.Services
{
    public class SpOrderService : BaseService, ISpOrderService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;

        public SpOrderService(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userSession = userSession;
        }


        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <param name="YEAR"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request, string YEAR)
        {
            var res = _dbContext.Query<SP_ORDER>();
            if (!string.IsNullOrEmpty(YEAR))
            {
                res = res.Where(t => t.ORDER_DATE.Value.Year.Equals(YEAR));
            }
            return await res.GetGridData(request);
        }

        class SpOrderRes : SP_ORDER
        {
            /// <summary>
            /// 订单状态
            /// </summary>
            public string STATUS;
        }
        public async Task<GridData> OrderListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_ORDER_DETAIL>()
                .Select(b => new
                {
                    b.ORDER_ID,
                    b.INVOICE_NUM,
                    b.RECEIVE_COUNT,
                    b.COUNT
                })
                .GroupBy(b => new
                {
                    b.ORDER_ID
                })
                .Select(b => new
                {
                    b.ORDER_ID,
                    INVOICE_NUM = Sql.Sum(b.INVOICE_NUM) ?? 0,
                    RECEIVE_COUNT = Sql.Sum(b.RECEIVE_COUNT) ?? 0,
                    COUNT = Sql.Sum(b.COUNT) ?? 0
                })
                .RightJoin<SP_ORDER>((b, c) => c.ORDER_ID == b.ORDER_ID)
                .Select((b, c) => new SpOrderRes
                {
                    STATUS = c.IS_STOP == "1" ? "7" : b.RECEIVE_COUNT == 0 ? "1" : b.RECEIVE_COUNT < b.COUNT ? "2" : b.INVOICE_NUM == 0 ? "4" : b.INVOICE_NUM < b.COUNT ? "5" : "6",
                    IS_DONE = c.IS_DONE,
                    PURPLAN_ID = c.PURPLAN_ID,
                    AUDITING = c.AUDITING,
                    REF_PROCESS = c.REF_PROCESS,
                    ORDER_CODE = c.ORDER_CODE,
                    BUY_USERID = c.BUY_USERID,
                    BUY_USER = c.BUY_USER,
                    ORDER_DATE = c.ORDER_DATE,
                    PROVIDER_ID = c.PROVIDER_ID,
                    PROVIDER_NAME = c.PROVIDER_NAME,
                    ORDER_MONEY = c.ORDER_MONEY,
                    VALID_ENDDATE = c.VALID_ENDDATE,
                    MEMO = c.MEMO,
                    SEC_DEPTID = c.SEC_DEPTID,
                    SEC_DEPT = c.SEC_DEPT,
                    ORDER_ID = c.ORDER_ID,
                    CREATE_USERID = c.CREATE_USERID,
                    CREATEDATE = c.CREATEDATE,
                    MODIFY_USERID = c.MODIFY_USERID,
                    MODIFYDATE = c.MODIFYDATE,
                    IS_CHK = c.IS_CHK,
                    AUDIT_USERID = c.AUDIT_USERID,
                    AUDIT_DATE = c.AUDIT_DATE,
                    EDIT_USER = c.EDIT_USER,
                    EDIT_USERID = c.EDIT_USERID,
                    EDIT_DATE = c.EDIT_DATE,
                    AUDIT_USER = c.AUDIT_USER,
                    INVOICE_TYPE = c.INVOICE_TYPE,
                    ORDER_TYPE = c.ORDER_TYPE,
                    CHK_MEMO = c.CHK_MEMO,
                    DONE_DATE = c.DONE_DATE,
                    DEPT_ID = c.DEPT_ID,
                    DEPT_NAME = c.DEPT_NAME,
                    REQUEST_ID = c.REQUEST_ID,
                    OA_DATE = c.OA_DATE,
                    FP_DONE = c.FP_DONE,
                    FP_DATE = c.FP_DATE,
                    IS_OLD = c.IS_OLD,
                    TAX_RATE = c.TAX_RATE,
                    OLD_INV = c.OLD_INV,
                    OVERDUE = c.OVERDUE,
                    INVOICE_MONEY = c.INVOICE_MONEY,
                    IS_STOP = c.IS_STOP,
                    REF_REQUEST = c.REF_REQUEST,
                    REQUEST_NAME = c.REQUEST_NAME,
                    BUY_USERDEPTID = c.BUY_USERDEPTID,
                    POSTADDRESS = c.POSTADDRESS,
                    PHONE = c.PHONE,
                    OACODE = c.OACODE,
                    MOBILE = c.MOBILE
                })
                .GetGridData(request);
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
                     { "BCCode", "order_src" },
                     { "ProviderName", (Expression<Func<PROVIDER, bool>>)null},
                });
                var dic1 = await _comboxDataService.Get(new Dictionary<string, object>()
                {
                    { "BCCode", "order_state" }
                });
                dic.TryAdd("OrderStatus", dic1["BCCode"]);
                return AjaxResult.Success(dic);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }


        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> Save(SaveRequest<SP_ORDER> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.IS_DONE,
                    c.PURPLAN_ID,
                    c.AUDITING,
                    c.REF_PROCESS,
                    c.ORDER_CODE,
                    c.BUY_USERID,
                    c.BUY_USER,
                    c.ORDER_DATE,
                    c.PROVIDER_ID,
                    c.PROVIDER_NAME,
                    c.ORDER_MONEY,
                    c.VALID_ENDDATE,
                    c.MEMO,
                    c.SEC_DEPTID,
                    c.SEC_DEPT,
                    c.ORDER_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.IS_CHK,
                    c.AUDIT_USERID,
                    c.AUDIT_DATE,
                    c.EDIT_USER,
                    c.EDIT_USERID,
                    c.EDIT_DATE,
                    c.AUDIT_USER,
                    c.INVOICE_TYPE,
                    c.ORDER_TYPE,
                    c.CHK_MEMO,
                    c.DONE_DATE,
                    c.DEPT_ID,
                    c.DEPT_NAME,
                    c.REQUEST_ID,
                    c.OA_DATE,
                    c.FP_DONE,
                    c.FP_DATE,
                    c.IS_OLD,
                    c.TAX_RATE,
                    c.OLD_INV,
                    c.OVERDUE,
                    c.INVOICE_MONEY,
                    c.IS_STOP,
                    c.REF_REQUEST,
                    c.REQUEST_NAME,
                    c.BUY_USERDEPTID,
                    c.POSTADDRESS,
                    c.PHONE,
                    c.OACODE,
                    c.MOBILE
                },
                c => a => a.ORDER_ID == c.ORDER_ID, BeforeAdd, BeforeUpdate, BeforeDelete);
        }

        private async Task BeforeAdd(SP_ORDER entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.ORDER_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
            entity.IS_STOP = "0";
            entity.AUDITING = "0";
        }

        private async Task BeforeUpdate(SP_ORDER entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();
            entity.DEPT_ID = string.IsNullOrEmpty(entity.DEPT_ID) ? _userSession.Corp.CorpID : entity.DEPT_ID;
            entity.DEPT_NAME = string.IsNullOrEmpty(entity.DEPT_NAME) ? _userSession.Corp.CName : entity.DEPT_NAME;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }

        private async Task BeforeDelete(SP_ORDER entity)
        {
            await _dbContext.DeleteAsync<SP_ORDER_DETAIL>(x => x.ORDER_ID == entity.ORDER_ID);
        }

        public async Task<int> Submit(List<string> sids)
        {
            var updatedevice = await _dbContext.UpdateAsync<SP_ORDER>(x => sids.Contains(x.ORDER_ID),
                    x => new SP_ORDER
                    {
                        AUDITING = "1",
                        BUY_USER = _userSession.RealName,
                        BUY_USERID = _userSession.UserID.ToString(),
                        BUY_USERDEPTID = _userSession.Corp.CorpID
                    });

            //数据同步到物资收货中
            var order = _dbContext.Query<SP_ORDER>().Where(t => sids.Contains(t.ORDER_ID)).ToList();
            foreach (var s in order)
            {
                var dets = _dbContext.Query<SP_ORDER_DETAIL>().Where(t => t.ORDER_ID == s.ORDER_ID).ToList();
                var depts = dets.Select(t => new { t.DEPT_ID, t.DEPT_NAME }).Distinct().ToList();
                foreach (var dept in depts)
                {
                    string type = "DJ" + DateTime.Now.ToString("yyyyMM");
                    string def = type + "0000";
                    var model = await _dbContext.Query<SP_RECEIVE>(x => x.RECEIVE_CODE.Contains(type)).Select(x => Sql.Max(x.RECEIVE_CODE) ?? def).FirstOrDefaultAsync();
                    var index = model.SubStr(8, 4).CastTo<int>() + 1;

                    var det = dets.Where(t => t.DEPT_ID == dept.DEPT_ID).ToList();
                    var apply = det.FirstOrDefault();
                    var data = new SP_RECEIVE
                    {
                        RECEIVE_ID = GuidHelper.NewSnowflakeId().ToString(),
                        USER_NAME = _userSession.UserName.ToString(),
                        USER_ID = _userSession.UserID.ToString(),
                        RECEIVE_CODE = type + index.ToString("D4"),
                        CREATEDATE = DateTime.Now,
                        CREATE_USERID = _userSession.UserID.ToString(),
                        AUDITING = "0",
                        PROVIDER_NAME = s.PROVIDER_NAME,
                        PROVIDER_ID = s.PROVIDER_ID,
                        PUR_USER = s.BUY_USER,
                        PUR_USERID = s.BUY_USERID,
                        ORDER_ID = s.ORDER_ID,
                        ORDER_CODE = s.ORDER_CODE,
                        DEPT_NAME = dept.DEPT_NAME,
                        DEPT_ID = dept.DEPT_ID,
                        CHK_USER = apply?.APPLY_USER,
                        CHK_USERID = apply?.APPLY_USERID
                    };
                    var spReceiveDet = new List<SP_RECEIVE_DET>();
                    foreach (var item in det)
                    {
                        var req = item.MapTo<SP_RECEIVE_DET>();
                        req.RECEIVE_ID = data.RECEIVE_ID;
                        req.RECDET_ID = GuidHelper.NewSnowflakeId().ToString();
                        req.CREATEDATE = DateTime.Now;
                        req.CREATE_USERID = _userSession.UserID.ToString();

                        spReceiveDet.Add(req);
                    }
                    await Task.CompletedTask;
                    await _dbContext.InsertAsync<SP_RECEIVE>(data);
                    await _dbContext.InsertRangeAsync<SP_RECEIVE_DET>(spReceiveDet);
                }

                var appledetId = dets.Select(t => t.SPDET_ID).ToList();
                await _dbContext.UpdateAsync<SP_APPLY_DETAIL>(x => appledetId.Contains(x.SPDET_ID),
                 x => new SP_APPLY_DETAIL
                 {
                     SP_STATUS = "50"//供货中
                 });
            }

            return updatedevice;
        }

        /// <summary>
        /// 撤销提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        public async Task<AjaxResult> CancelSubmit(List<string> sids)
        {
            var list = _dbContext.Query<SP_ORDER>().Where(t => sids.Contains(t.ORDER_ID)).ToList();

            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    if (_dbContext.Query<SP_RECEIVE>().Any(t => t.ORDER_ID == item.ORDER_ID && t.AUDITING == "1"))
                    {
                        throw new Exception($"{item.ORDER_CODE}供货中,不能撤销!");
                    }
                }

                var updatedevice = await _dbContext.UpdateAsync<SP_ORDER>(x => sids.Contains(x.ORDER_ID),
                   x => new SP_ORDER
                   {
                       AUDITING = "0"
                   });

                var data = _dbContext.Query<SP_ORDER_DETAIL>().Where(x => sids.Contains(x.ORDER_ID)).Select(t => t.SPDET_ID).ToList();
                await _dbContext.UpdateAsync<SP_APPLY_DETAIL>(x => data.Contains(x.SPDET_ID),
                x => new SP_APPLY_DETAIL
                {
                    SP_STATUS = "40"//请购中
                });

                var orderId = _dbContext.Query<SP_RECEIVE>().Where(t => sids.Contains(t.ORDER_ID)).Select(t => t.RECEIVE_ID).ToList();
                await _dbContext.DeleteAsync<SP_RECEIVE>(x => orderId.Contains(x.RECEIVE_ID));
                await _dbContext.DeleteAsync<SP_RECEIVE_DET>(x => orderId.Contains(x.RECEIVE_ID));

            }
            return AjaxResult.Success("成功");
        }

        /// <summary>
        /// 获取明细列表信息
        /// </summary>
        /// <param name="ORDER_ID"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> DetailListAsync(string ORDER_ID, GridRequest request)
        {
            return await _dbContext.Query<SP_ORDER_DETAIL>().Where(t => t.ORDER_ID == ORDER_ID).GetGridData(request);
        }
        /// <summary>
        /// 明细保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> DetailSave(SaveRequest<SP_ORDER_DETAIL> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.SP_ID,
                    c.SP_CODE,
                    c.SP_NAME,
                    c.SP_SIZE,
                    c.UNIT,
                    c.PRODUCE,
                    c.COUNT,
                    c.PRICE,
                    c.MONEY,
                    c.REQ_DATE,
                    c.APPLY_NO,
                    c.ARRIVAL_COUNT,
                    c.INSTORE_COUNT,
                    c.BUY_WAY,
                    c.PROJECT_CODE,
                    c.TYPE_ID,
                    c.TYPE_CODE,
                    c.TYPE_NAME,
                    c.MEMO,
                    c.ORDERDET_ID,
                    c.SPDET_ID,
                    c.ORDER_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.INVOICE_NUM,
                    c.APPLY_MEMO,
                    c.USE_MEMO,
                    c.DEPT_ID,
                    c.DEPT_NAME,
                    c.NOTAX_PRICE,
                    c.INSTORE_TIMES,
                    c.INVOICE_TIMES,
                    c.APPLY_USERID,
                    c.APPLY_USER,
                    c.IS_STOP,
                    c.T_MEMO,
                    c.RECEIVE_COUNT,
                    c.PLAN_ID,
                    c.STOP_USERID,
                    c.STOP_USER,
                    c.STOP_DATE,
                    c.PERIOD,
                    c.IN_DATE,
                    c.INVOICE_DATE,
                    c.RECEIVE_DATE,
                    c.STOP_NUM,
                    c.IN_DONE,
                    c.FP_DONE,
                    c.UNTAX_MONEY,
                    c.TAX_MONEY,
                    c.SUM_MONEY,
                    c.OVERDUE,
                    c.WARRANTY,
                    c.INDET_ID,
                    c.IS_FALSE,
                    c.FIC_ID,
                    c.DELIVERY_CODE,
                    c.FIC_DETID,
                    c.TAX_RATE,
                    c.SYDD,
                    c.COUNT2,
                    c.PRICE2,
                    c.STOP_NUM2,
                    c.RECEIVE_COUNT2,
                    c.ARRIVAL_COUNT2,
                    c.INSTORE_COUNT2,
                    c.INVOICE_NUM2,
                    c.ADD_NUM
                },
                c => a => a.ORDERDET_ID == c.ORDERDET_ID, DetBeforeAdd, DetBeforeUpdate);
        }

        private async Task DetBeforeAdd(SP_ORDER_DETAIL entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.ORDERDET_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
            entity.IS_STOP = "0";
        }

        private async Task DetBeforeUpdate(SP_ORDER_DETAIL entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }

        public async Task<GridData> OrderOverListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_ORDER_DETAIL>()
                .LeftJoin<SP_ORDER>((a, b) => a.ORDER_ID == b.ORDER_ID)
                .Where((a, b) => a.OVERDUE == "1")
                .Select((a, b) => new
                {
                    b.ORDER_CODE,
                    b.ORDER_DATE,
                    a.REQ_DATE,
                    a.SP_ID,
                    a.SP_CODE,
                    a.SP_NAME,
                    a.SP_SIZE,
                    a.PRODUCE,
                    a.UNIT,
                    a.COUNT,
                    a.INSTORE_COUNT,
                    a.RECEIVE_COUNT,
                    a.STOP_NUM,
                    a.APPLY_USER,
                    a.DEPT_NAME,
                    a.USE_MEMO,
                    a.ORDERDET_ID,
                })
                .GetGridData(request);
        }

        /// <summary>
        /// 导出模板数据
        /// </summary>
        /// <param name="request"></param>
        /// <param name="YEAR"></param>
        /// <returns></returns>
        public async Task<GridData> ExportListAsync(GridRequest request, string YEAR)
        {
            var query = _dbContext.Query<SP_ORDER>().Where(t => t.AUDITING == "1");
            if (!string.IsNullOrEmpty(YEAR))
            {
                query = query.Where(t => t.ORDER_DATE.Value.Year.Equals(YEAR));
            }
            var res = await query.Select(t => new OrderExportData
            {
                ORDER_CODE = t.ORDER_CODE,
                ORDER_TYPE = t.ORDER_TYPE,
                ORDER_DATE = t.ORDER_DATE,
                DEPT_NAME = t.DEPT_NAME,
                ORDER_MONEY = t.ORDER_MONEY,
                PROVIDER_NAME = t.PROVIDER_NAME
            })
                .GetGridData(request);
            var dic = _dbContext.Query<BC_CODE>().Where(c => c.CODE_TYPE == "order_src").ToList();
            foreach (var item in (List<OrderExportData>)res.Rows)
            {
                item.ORDER_TYPE = dic.Where(t => t.CODE_EN == item.ORDER_TYPE).FirstOrDefault()?.CODE_CN;
                item.ORDER_DATESTR = item.ORDER_DATE.Value.ToString("yyyy-MM-dd");
            }
            return res;
        }
    }
}
