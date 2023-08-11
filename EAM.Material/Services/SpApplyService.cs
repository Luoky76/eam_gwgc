using Chloe;
using Chloe.MySql;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using EAM.Material.Interfaces;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Magicodes.ExporterAndImporter.Core.Models;
using NPOI.OpenXmlFormats.Dml.Diagram;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using WkHtmlToPdfDotNet;

namespace EAM.Material.Services
{
    public class SpApplyService : BaseService, ISpApplyService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;

        public SpApplyService(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userSession = userSession;
        }

        #region 采购申请
        class SpApplyRes : SP_APPLY
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
            var res = await _dbContext.Query<SP_APPLY>()
                .Select(c => new SpApplyRes
                {
                    APPLY_ID = c.APPLY_ID,
                    AUDITING = c.AUDITING,
                    APPLY_NO = c.APPLY_NO,
                    TYPE_ID = c.TYPE_ID,
                    USE_MEMO = c.USE_MEMO,
                    EXIG_DEV = c.EXIG_DEV,
                    APPLY_USER = c.APPLY_USER,
                    DEPT_ID = c.DEPT_ID,
                    DEPT_NAME = c.DEPT_NAME,
                    SEC_DEPTID = c.SEC_DEPTID,
                    SEC_DEPT = c.SEC_DEPT,
                    APPLY_DATE = c.APPLY_DATE,
                    CREATE_USERID = c.CREATE_USERID,
                    CREATEDATE = c.CREATEDATE,
                    TYPE_ID2=c.TYPE_ID2,
                    CGFS=c.CGFS,
                    TYPE_CODE=c.TYPE_CODE,
                    TYPE_NAME=c.TYPE_NAME,
                    MEMO = c.MEMO
                })
                .GetGridData(request);
            foreach (var item in (List<SpApplyRes>)res.Rows)
            {
                item.DETAILCOUNT = _dbContext.Query<SP_APPLY_DETAIL>().Where(t => t.APPLY_ID == item.APPLY_ID).Count();
            }
            return res;
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
                    { "BCCode", "exig_dev" },
                    { "BasePurtype", (Expression<Func<BASE_PURTYPE, bool>>)null}
                });
                var dic1 = await _comboxDataService.Get(new Dictionary<string, object>()
                {
                    { "BCCode", "CGtype" }
                });
                dic.TryAdd("CGFS", dic1["BCCode"]);
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
        public async Task<AjaxResult> Save(SaveRequest<SP_APPLY> request)
        {
            await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.AUDITING,
                    c.APPLY_NO,
                    c.APPLY_DATE,
                    c.APPLY_USERID,
                    c.APPLY_USER,
                    c.DEPT_ID,
                    c.DEPT_CODE,
                    c.DEPT_NAME,
                    c.IS_REC,
                    c.TIME_REQ,
                    c.SOURCE_ID,
                    c.SOURCE,
                    c.USE_MEMO,
                    c.TYPE_ID,
                    c.TYPE_CODE,
                    c.TYPE_NAME,
                    c.EXIG_DEV,
                    c.PROJECT_CODE,
                    c.OA_CHECK,
                    c.OA_DATE,
                    c.OA_MEMO,
                    c.SEC_DEPTID,
                    c.REQUEST_ID,
                    c.SEC_DEPT,
                    c.MEMO,
                    c.APPLY_ID,
                    c.IS_GEN,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.SUM_MONEY,
                    c.CGFS,
                    c.TYPE_ID2,
                    c.SSZT,
                    c.SSZTID,
                    c.BD_NAME
                },
                c => a => a.APPLY_ID == c.APPLY_ID, BeforeAdd, BeforeUpdate);

            var id = "";
            if (request.Added?.Count > 0)
                id = request.Added[0].APPLY_ID;

            return AjaxResult.Success(id);
        }

        private async Task BeforeAdd(SP_APPLY entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.APPLY_ID = GuidHelper.NewSnowflakeId().ToString();

            string type = $"SQ{DateTime.Now.ToString("yyyyMM")}";
            string def = type + "0000";
            var model = await _dbContext.Query<SP_APPLY>(x => x.APPLY_NO.Contains(type)).Select(x => Sql.Max(x.APPLY_NO) ?? def).FirstOrDefaultAsync();
            var index = model.SubStr(8, 4).CastTo<int>() + 1;
            entity.APPLY_NO = type + index.ToString("D4");

            entity.APPLY_DATE = dt;
            entity.APPLY_USERID = _userSession.UserID.ToString();
            entity.APPLY_USER = _userSession.RealName;
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

        private async Task BeforeUpdate(SP_APPLY entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;

        }


        public async Task<int> Submit(List<string> sids)
        {
            var updatedevice = await _dbContext.UpdateAsync<SP_APPLY>(x => sids.Contains(x.APPLY_ID),
                    x => new SP_APPLY
                    {
                        AUDITING = "1"
                    });
            await _dbContext.UpdateAsync<SP_APPLY_DETAIL>(x => sids.Contains(x.APPLY_ID),
                   x => new SP_APPLY_DETAIL
                   {
                       SP_STATUS = "20"//待请购
                   });

            var list = _dbContext.Query<SP_APPLY>().Where(x => sids.Contains(x.APPLY_ID) && x.CGFS == "逐单采购").ToList();
            //逐单采购模式订单直接生成询价方案
            if (list.Count > 0)
            {
                DateTime? dt = await _dbContext.GetSysdate();
                var importDetail = new List<SP_PURPLAN_DET>();
                var importList = new List<SP_PURPLAN>();
                //单号
                string type = $"XJ{dt.Value.ToString("yyyyMM")}";
                string def = type + "0000";
                var model = await _dbContext.Query<SP_PURPLAN>(x => x.PLAN_NO.Contains(type)).Select(x => Sql.Max(x.PLAN_NO) ?? def).FirstOrDefaultAsync();
               
                var i = 1;
                foreach (var item in list)
                {
                    var index = model.SubStr(8, 4).CastTo<int>() + i;
                    //形成物资询价方案
                    var temp = item.MapTo<SP_PURPLAN>();
                    temp.PURPLAN_ID = GuidHelper.NewSnowflakeId().ToString();

                    temp.PLAN_NO = type + index.ToString("D4");
                    temp.PLAN_DATE = dt;
                    temp.ID_URGENT_PURCHASE = item.EXIG_DEV == "1" ? "1" : "0";
                    temp.CREATE_USERID = _userSession.UserID.ToString();
                    temp.CREATEDATE = dt;
                    temp.MODIFY_USERID = _userSession.UserID.ToString();
                    temp.MODIFYDATE = dt;
                    temp.AUDITING = "0";
                    importList.Add(temp);
                    i++;
                    await Task.CompletedTask;

                    var data = _dbContext.Query<SP_APPLY_DETAIL>().Where(x => x.APPLY_ID == item.APPLY_ID).ToList();
                    foreach (var det in data)
                    {
                        var req = det.MapTo<SP_PURPLAN_DET>();
                        req.APPLY_ID = item.APPLY_ID;
                        req.APPLY_NO = item.APPLY_NO;
                        req.APPLY_DATE = item.APPLY_DATE;
                        req.DEPT_ID = item.DEPT_ID;
                        req.DEPT_CODE = item.DEPT_CODE;
                        req.DEPT_NAME = item.DEPT_NAME;
                        req.SEC_DEPTID = item.SEC_DEPTID;
                        req.SEC_DEPT = item.SEC_DEPT;
                        req.EXIG_DEV = item.EXIG_DEV;
                        req.USE_MEMO = item.USE_MEMO;
                        req.APPLY_USERID = item.APPLY_USERID;
                        req.APPLY_USER = item.APPLY_USER;

                        req.PURPLAN_ID = temp.PURPLAN_ID;

                        req.PLAN_ID = GuidHelper.NewSnowflakeId().ToString();
                        req.CREATE_USERID = _userSession.UserID.ToString();
                        req.CREATEDATE = dt;
                        req.MODIFY_USERID = _userSession.UserID.ToString();
                        req.MODIFYDATE = dt;
                        req.STATUS = "1";
                        importDetail.Add(req);
                        await Task.CompletedTask;

                        det.SP_STATUS = "30";
                        await _dbContext.UpdateAsync<SP_APPLY_DETAIL>(det);
                    }                 
                }

                await _dbContext.InsertRangeAsync<SP_PURPLAN>(importList);
                await _dbContext.InsertRangeAsync<SP_PURPLAN_DET>(importDetail);
            }

            return updatedevice;
        }

        /// <summary>
        /// 明细-列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> DetailListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_APPLY_DETAIL>().GetGridData(request);
        }

        /// <summary>
        /// 明细-保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> DetailSave(SaveRequest<SP_APPLY_DETAIL> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.SPDET_ID,
                    c.APPLY_ID,
                    c.SP_ID,
                    c.SP_CODE,
                    c.SP_NAME,
                    c.SP_SIZE,
                    c.PRODUCE,
                    c.UNIT,
                    c.COUNT,
                    c.STORE_NUM,
                    c.YG_PRICE,
                    c.YG_MONEY,
                    c.LAST_PROVIDERID,
                    c.LAST_PROVIDER,
                    c.TYPE_ID,
                    c.TYPE_CODE,
                    c.TYPE_NAME,
                    c.IS_STOP,
                    c.MEMO,
                    c.IS_GEN,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.TENANT_ID,
                    c.PURTYPE_ID,
                    c.PURTYPE_NAME,
                    c.IS_CANCEL,
                    c.NO_PRODUCE,
                    c.COMP_CODE,
                    c.STORE_MONTH,
                    c.PUR_PERIOD,
                    c.ONROAD_NUM,
                    c.PRO_ID,
                    c.PRO_DET_ID,
                    c.PERIOD,
                    c.IS_XY,
                    c.WARRANTY,
                    c.DELIVERY_CODE,
                    c.XHZQ,
                    c.SYDD,
                    c.CGFS,
                    c.SYDDDEPTID,
                    c.COUNT2,
                    c.CGFS2,
                    c.SP_CODE2,
                    c.COMP_CODE2,
                    c.SP_NAME2,
                    c.SYDD2,
                    c.SYDDDEPTID2,
                    c.SP_SIZE2,
                    c.PRODUCE2,
                    c.UNIT2,
                    c.ZKCS,
                    c.QYKCSL
                },
                c => a => a.SPDET_ID == c.SPDET_ID, BeforeAddDet, BeforeUpdateDet,null,false,null, AfterSaveDet);
        }

        private async Task BeforeAddDet(SP_APPLY_DETAIL entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.SPDET_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
            entity.SP_STATUS = "10";//计划
        }

        private async Task BeforeUpdateDet(SP_APPLY_DETAIL entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;

        }

        private async Task AfterSaveDet(List<SP_APPLY_DETAIL> added, List<SP_APPLY_DETAIL> updated, List<SP_APPLY_DETAIL> deleted)
        {
            var applyId = added.Count == 0 ? updated.Count == 0 ? deleted.Select(c => c.APPLY_ID).FirstOrDefault() : updated.Select(c => c.APPLY_ID).FirstOrDefault() : added.Select(c => c.APPLY_ID).FirstOrDefault();
            await Task.CompletedTask;
            if (!string.IsNullOrEmpty(applyId))
            {
                await _dbContext.UpdateAsync<SP_APPLY>(x => x.APPLY_ID == applyId,
                    x => new SP_APPLY
                    {
                        SUM_MONEY = _dbContext.Query<SP_APPLY_DETAIL>().Where(t => t.APPLY_ID == applyId).Sum(t => t.YG_MONEY)
                    });
            }
        }
        #endregion

        #region 采购进度跟踪
        public class SpApplyDetRes : SP_APPLY_DETAIL
        {
            /// <summary>
            /// 紧急程度
            /// </summary>
            public string EXIG_DEV;

            public string APPLY_USER;

            public DateTime? APPLY_DATE;

            public string DEPT_NAME;
            public string SEC_DEPT;
        }
        public async Task<GridData> ApplyListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_APPLY_DETAIL>()
                 .LeftJoin<SP_APPLY>((a,b)=>a.APPLY_ID == b.APPLY_ID)
                 .Where((a, b) =>b.AUDITING == "1")
                .Select((a, b) => new SpApplyDetRes
                {
                    SP_STATUS = a.SP_STATUS,
                    SP_CODE = a.SP_CODE,
                    SP_NAME = a.SP_NAME,
                    SP_SIZE = a.SP_SIZE,
                    PRODUCE = a.PRODUCE,
                    UNIT = a.UNIT,
                    TYPE_NAME = a.TYPE_NAME,
                    IS_XY = a.IS_XY,
                    EXIG_DEV = b.EXIG_DEV,
                    APPLY_USER = b.APPLY_USER,
                    APPLY_DATE = b.APPLY_DATE,
                    DEPT_NAME = b.DEPT_NAME,
                    SEC_DEPT = b.SEC_DEPT,
                    MEMO = a.MEMO,
                    SPDET_ID = a.SPDET_ID
                })
                .GetGridData(request);
        }

        public async Task<AjaxResult> ApplyComboxData()
        {
            try
            {
                var dic = await _comboxDataService.Get(new Dictionary<string, object>()
                {
                    { "BCCode", "exig_dev" },
                });
                var dic1 = await _comboxDataService.Get(new Dictionary<string, object>()
                {
                    { "BCCode", "pur_state" }
                });
                dic.TryAdd("StatusData", dic1["BCCode"]);
                return AjaxResult.Success(dic);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }

        public class SpApplyDetFlowRes
        {
            public string SPDET_ID;
            public string SP_STATUS;
            public string SP_CODE;
            public string SP_NAME;

            public string SP_SIZE;
            public string PRODUCE;
            public string UNIT;
            public string TYPE_NAME;
            public string APPLY_NO;
            public string COLLECT_CODE;
            public string PLAN_NO; 
            public string ORDER_CODE;
            public string APPLY_USER; 
            public decimal? APPLY_COUNT;
            public string PROVIDER_NAME; 
            public string DEPT_NAME;
            public string XJDOWN_USER;
            public string BUY_USER;
            public string PERIOD;
            public decimal? COUNT;
            public decimal? INSTORE_COUNT;
            public decimal? STOP_NUM;
            public decimal? YG_PRICE;
            public decimal? PRICE;


            public DateTime? APPLY_DATE;
            public DateTime? COLLECT_DATE;
            public DateTime? PLAN_DATE;
            public DateTime? ORDER_DATE;
            public DateTime? STOP_DATE;
            public string T_MEMO;
        }
        public async Task<AjaxResult> ApplyDetFlowAsync(string SPDET_ID)
        {
            var applydet = _dbContext.Query<SP_APPLY_DETAIL>()
                 .LeftJoin<SP_APPLY>((a, b) => a.APPLY_ID == b.APPLY_ID)
                 .Where((a, b) => a.SPDET_ID == SPDET_ID)
                .Select((a, b) => new SpApplyDetFlowRes
                {
                    SP_STATUS = a.SP_STATUS,
                    SP_CODE = a.SP_CODE,
                    SP_NAME = a.SP_NAME,
                    SP_SIZE = a.SP_SIZE,
                    PRODUCE = a.PRODUCE,
                    UNIT = a.UNIT,
                    TYPE_NAME = a.TYPE_NAME,
                    APPLY_COUNT = a.COUNT,
                    APPLY_NO = b.APPLY_NO,
                    APPLY_USER = b.APPLY_USER,
                    APPLY_DATE = b.APPLY_DATE,
                    SPDET_ID = a.SPDET_ID,
                    YG_PRICE = a.YG_PRICE
                }).First();

            var col = _dbContext.Query<SP_COLLECT_REQUEST>()
                .LeftJoin<SP_COLLECT>((a, b) => a.COLLECT_ID == b.COLLECT_ID)
                 .Where((a, b) => a.REQUEST_DET_ID == SPDET_ID)
                 .Select((a, b) => new
                 {
                     b.COLLECT_CODE,
                     b.COLLECT_DATE
                 }).FirstOrDefault();

            var pur = _dbContext.Query<SP_PURPLAN_DET>()
             .LeftJoin<SP_PURPLAN>((a, b) => a.PURPLAN_ID == b.PURPLAN_ID)
              .Where((a, b) => a.SPDET_ID == SPDET_ID)
              .Select((a, b) => new
              {
                  b.PLAN_NO,
                  b.PLAN_DATE,
                  b.XJDOWN_USER,
                  a.PERIOD
              }).FirstOrDefault();

            var order = await _dbContext.Query<SP_ORDER_DETAIL>()
             .LeftJoin<SP_ORDER>((a, b) => a.ORDER_ID == b.ORDER_ID)
              .Where((a, b) => a.SPDET_ID == SPDET_ID)
              .Select((a, b) => new
              {
                  b.ORDER_CODE,
                  b.ORDER_DATE,
                  a.STOP_DATE,
                  a.T_MEMO,
                  b.PROVIDER_NAME,
                  b.DEPT_NAME,
                  b.BUY_USER,
                  a.COUNT,
                  a.PRICE,
                  a.STOP_NUM,
                  a.INSTORE_COUNT
              }).FirstAsync();

            applydet.COLLECT_CODE = col?.COLLECT_CODE;
            applydet.COLLECT_DATE = col?.COLLECT_DATE;
            applydet.PLAN_NO = pur?.PLAN_NO;
            applydet.PLAN_DATE = pur?.PLAN_DATE;
            applydet.XJDOWN_USER = pur?.XJDOWN_USER;
            applydet.PERIOD = pur?.PERIOD;

            applydet.ORDER_CODE = order?.ORDER_CODE;
            applydet.ORDER_DATE = order?.ORDER_DATE;
            applydet.STOP_DATE = order?.STOP_DATE;
            applydet.T_MEMO = order?.T_MEMO;
            applydet.PROVIDER_NAME = order?.PROVIDER_NAME;
            applydet.DEPT_NAME = order?.DEPT_NAME;
            applydet.BUY_USER = order?.BUY_USER;
            applydet.COUNT = order?.COUNT;
            applydet.PRICE = order?.PRICE;
            applydet.STOP_NUM = order?.STOP_NUM;
            applydet.INSTORE_COUNT = order?.INSTORE_COUNT;

            return AjaxResult.Success(applydet);
        }
        #endregion
    }
}
