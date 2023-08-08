using DocumentFormat.OpenXml.Drawing.Charts;
using EAM.Material.Interfaces;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EAM.Material.Services
{
    public class SpOrderStopService : BaseService, ISpOrderStopService
    {
        private readonly IDbContext _dbContext;
        private readonly UserSession _userSession;

        public SpOrderStopService(IDbContext dbContext, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userSession = userSession;
        }

        class SpOrderStopRes : SP_ORDER_STOP
        {
            /// <summary>
            /// 填写的明细数量
            /// </summary>
            public int DETAILCOUNT;
        }
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var res = await _dbContext.Query<SP_ORDER_STOP>()
                 .Select(c => new SpOrderStopRes
                 {
                     AUDITING = c.AUDITING,
                     STOP_CODE = c.STOP_CODE,
                     USER_ID = c.USER_ID,
                     USER_NAME = c.USER_NAME,
                     DEPT_ID = c.DEPT_ID,
                     DEPT_NAME = c.DEPT_NAME,
                     SEC_DEPTID = c.SEC_DEPTID,
                     SEC_DEPT = c.SEC_DEPT,
                     EDIT_DATE = c.EDIT_DATE,
                     MEMO = c.MEMO,
                     STOP_ID = c.STOP_ID,
                     CREATE_USERID = c.CREATE_USERID,
                     CREATEDATE = c.CREATEDATE,
                     MODIFY_USERID = c.MODIFY_USERID,
                     MODIFYDATE = c.MODIFYDATE
                 }).GetGridData(request);
            foreach (var item in (List<SpOrderStopRes>)res.Rows)
            {
                item.DETAILCOUNT = _dbContext.Query<SP_STOP_DET>().Where(t => t.STOP_ID == item.STOP_ID).Count();
            }
            return res;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> Save(SaveRequest<SP_ORDER_STOP> request)
        {
             await _dbContext.SaveEntityAnsyc(request,
                c => new
                {

                    c.AUDITING,
                    c.STOP_CODE,
                    c.USER_ID,
                    c.USER_NAME,
                    c.DEPT_ID,
                    c.DEPT_NAME,
                    c.SEC_DEPTID,
                    c.SEC_DEPT,
                    c.EDIT_DATE,
                    c.MEMO,
                    c.STOP_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                },
                c => a => a.STOP_ID == c.STOP_ID, BeforeAdd, BeforeUpdate);
            var id = "";
            if (request.Added?.Count > 0)
                id = request.Added[0].STOP_ID;

            return AjaxResult.Success(id);
        }

        private async Task BeforeAdd(SP_ORDER_STOP entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.STOP_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.STOP_CODE = $"ZZ{dt.Value.ToString("yyyyMMddHHmmss")}";

            entity.EDIT_DATE = dt;
            entity.USER_ID = _userSession.UserID.ToString();
            entity.USER_NAME = _userSession.RealName;
            entity.SEC_DEPTID = _userSession.ParentCompany.CorpID;
            entity.SEC_DEPT = _userSession.ParentCompany.CName;
            entity.DEPT_ID = _userSession.Corp.CorpID;
            entity.DEPT_NAME = _userSession.Corp.CName;
            entity.AUDITING = "0";

            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }

        private async Task BeforeUpdate(SP_ORDER_STOP entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        public async Task<int> Submit(List<string> sids)
        {
            var updatedevice = await _dbContext.UpdateAsync<SP_ORDER_STOP>(x => sids.Contains(x.STOP_ID),
                    x => new SP_ORDER_STOP
                    {
                        AUDITING = "1"
                    });

            var list = _dbContext.Query<SP_ORDER_STOP>().Where(x => sids.Contains(x.STOP_ID)).ToList();
            var dets = _dbContext.Query<SP_STOP_DET>().Where(x => sids.Contains(x.STOP_ID)).ToList();

            if (dets.Count > 0)
            {
                //订单中止
                foreach (var det in dets)
                {
                    var stop = list.Where(t => t.STOP_ID == det.STOP_ID).First();
                    await _dbContext.UpdateAsync<SP_ORDER_DETAIL>(x => x.ORDERDET_ID == det.ORDERDET_ID,
                        x => new SP_ORDER_DETAIL
                        {
                            IS_STOP = "1",
                            T_MEMO = stop.MEMO,
                            STOP_USERID = stop.USER_ID,
                            STOP_USER = stop.USER_NAME,
                            STOP_DATE = stop.EDIT_DATE,
                            STOP_NUM = det.STOP_NUM
                        });
                }

                var orderIds = dets.Select(t => t.ORDER_ID).Distinct().ToList();

                foreach (var id in orderIds)
                {
                    var count = _dbContext.Query<SP_ORDER_DETAIL>().Where(x => x.ORDER_ID == id).Count();
                    var detcount = dets.Where(t => t.ORDER_ID == id).Count();

                    if (count == detcount)
                    {
                        await _dbContext.UpdateAsync<SP_ORDER>(x =>x.ORDER_ID == id,
                        x => new SP_ORDER
                        {
                            IS_STOP = "1"
                        });
                    }
                }
            }

            return updatedevice;
        }

        class SpOrderStopDetRes : SP_STOP_DET
        {
            /// <summary>
            /// 订单编号
            /// </summary>
            public string ORDER_CODE;
            public DateTime? ORDER_DATE;
            public string SP_CODE;
            public string SP_NAME;
            public string SP_SIZE;
            public decimal? COUNT;
            public decimal? INSTORE_COUNT;
            public string APPLY_USER;
            public string DEPT_NAME;
        }
        /// <summary>
        /// 获取明细列表信息
        /// </summary>
        /// <param name="STOP_ID"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> DetailListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_STOP_DET>()
                .LeftJoin<SP_ORDER_DETAIL>((a,b)=>a.ORDERDET_ID == b.ORDERDET_ID)
                 .LeftJoin<SP_ORDER>((a, b,c) => a.ORDER_ID == c.ORDER_ID)
                 .Select((a, b, c) => new SpOrderStopDetRes
                 {
                     ORDER_CODE = c.ORDER_CODE,
                     ORDER_DATE = c.ORDER_DATE,
                     SP_CODE = b.SP_CODE,
                     SP_NAME = b.SP_NAME,
                     SP_SIZE = b.SP_SIZE,
                     COUNT = b.COUNT,
                     INSTORE_COUNT = b.INSTORE_COUNT,
                     APPLY_USER = b.APPLY_USER,
                     DEPT_NAME = b.DEPT_NAME,
                     STOP_NUM = a.STOP_NUM,
                     ORDER_ID = a.ORDER_ID,
                     ORDERDET_ID = a.ORDERDET_ID,
                     STOP_DET_ID = a.STOP_DET_ID,
                     STOP_ID = a.STOP_ID,
                     CREATE_USERID = a.CREATE_USERID,
                     CREATEDATE = a.CREATEDATE,
                     MODIFY_USERID = a.MODIFY_USERID,
                     MODIFYDATE = a.MODIFYDATE
                 })
                .GetGridData(request);
        }
        /// <summary>
        /// 明细保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> DetailSave(SaveRequest<SP_STOP_DET> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.STOP_NUM,
                    c.ORDER_ID,
                    c.ORDERDET_ID,
                    c.STOP_DET_ID,
                    c.STOP_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.STOP_NUM2
                },
                c => a => a.STOP_DET_ID == c.STOP_DET_ID, DetBeforeAdd, DetBeforeUpdate);
        }

        private async Task DetBeforeAdd(SP_STOP_DET entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.ORDERDET_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }

        private async Task DetBeforeUpdate(SP_STOP_DET entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }

        public async Task<GridData> SpOrderListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_ORDER_DETAIL>()
                .LeftJoin<SP_ORDER>((a, b) => a.ORDER_ID == b.ORDER_ID)
                .Where((a, b) => a.IS_STOP == "0" && b.AUDITING == "1")
                .Select((a, b) => new
                {
                    a.ORDERDET_ID,
                    a.ORDER_ID,
                    b.ORDER_CODE,
                    a.SP_ID,
                    a.SP_CODE,
                    a.SP_NAME,
                    a.SP_SIZE,
                    a.PRODUCE,
                    a.UNIT,
                    a.TYPE_NAME
                })
                .GetGridData(request);
        }
    }
}
