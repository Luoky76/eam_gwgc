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
using Microsoft.CodeAnalysis;
using NPOI.OpenXmlFormats.Dml.Diagram;
using NPOI.Util;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq.Expressions;
using System.Reflection.Emit;
using WkHtmlToPdfDotNet;

namespace EAM.Material.Services
{
    public class SpCollectService : BaseService, ISpCollectService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;

        public SpCollectService(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userSession = userSession;
        }

        #region 请购申请
        class SpCollectRes : SP_COLLECT
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
            var res = await _dbContext.Query<SP_COLLECT>()
                .Select(c => new SpCollectRes
                {
                    COLLECT_ID = c.COLLECT_ID,
                    AUDITING = c.AUDITING,
                    COLLECT_CODE = c.COLLECT_CODE,
                    COLLECT_DATE = c.COLLECT_DATE,
                    COLLECT_USER = c.COLLECT_USER,
                    COLLECT_USERID = c.COLLECT_USERID,
                    DEPT_ID = c.DEPT_ID,
                    DEPT_NAME = c.DEPT_NAME,
                    SEC_DEPTID = c.SEC_DEPTID,
                    SEC_DEPT = c.SEC_DEPT,
                    COLLECT_PRICE = c.COLLECT_PRICE,
                    CREATE_USERID = c.CREATE_USERID,
                    CREATEDATE = c.CREATEDATE,
                    COLLECT_SPTYPE = c.COLLECT_SPTYPE,
                    COLLECT_METHOD = c.COLLECT_METHOD,
                    CONSULT_PROVIDER = c.CONSULT_PROVIDER,
                    MEMO = c.MEMO
                })
                .GetGridData(request);
            foreach (var item in (List<SpCollectRes>)res.Rows)
            {
                item.DETAILCOUNT = _dbContext.Query<SP_COLLECT_DET>().Where(t => t.COLLECT_ID == item.COLLECT_ID).Count();
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
                    { "BCCode", "CGtype" },
                    { "BaseSpType", (Expression<Func<BASE_SPTYPE, bool>>)null},
                    { "ProviderName", (Expression<Func<PROVIDER, bool>>)null},
                });
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
        public async Task<AjaxResult> Save(SaveRequest<SP_COLLECT> request)
        {
            await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.AUDITING,
                    c.COLLECT_ID,
                    c.COLLECT_CODE,
                    c.COLLECT_DATE,
                    c.COLLECT_USER,
                    c.COLLECT_USERID,
                    c.DEPT_ID,
                    c.DEPT_NAME,
                    c.SEC_DEPT,
                    c.SEC_DEPTID,
                    c.HOUSE_NAME,
                    c.HOUSE_ID,
                    c.MEMO,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.COLLECT_METHOD,
                    c.EDIT_USER,
                    c.COLLECT_PRICE,
                    c.CONFIRM_AUDIT,
                    c.TAX_MONEY,
                    c.NOTAX_MONEY,
                    c.CONFIRM_CODE,
                    c.CONFIRM_DATE,
                    c.PROVIDER_CODE,
                    c.PROVIDER_ID,
                    c.PROVIDER_NAME,
                    c.HOUSE_CODE,
                    c.STORE_TYPE,
                    c.HOUSE_USER,
                    c.HOUSE_USERID,
                    c.COLLECT_SPTYPE,
                    c.RATIO,
                    c.CONSULT_PROVIDER,
                    c.BD_NO
                },
                c => a => a.COLLECT_ID == c.COLLECT_ID, BeforeAdd, BeforeUpdate, BeforeDelete);

            var id = "";
            if (request.Added?.Count > 0)
                id = request.Added[0].COLLECT_ID;

            return AjaxResult.Success(id);
        }

        private async Task BeforeAdd(SP_COLLECT entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.COLLECT_ID = GuidHelper.NewSnowflakeId().ToString();
            //单号
            string type = $"QG{dt.Value.ToString("yyyyMM")}";
            string def = type + "0000";
            var model = await _dbContext.Query<SP_COLLECT>(x => x.COLLECT_CODE.Contains(type)).Select(x => Sql.Max(x.COLLECT_CODE) ?? def).FirstOrDefaultAsync();
            var index = model.SubStr(8, 4).CastTo<int>() + 1;

            entity.COLLECT_CODE = type + index.ToString("D4");

            entity.COLLECT_DATE = dt;
            entity.COLLECT_USERID = _userSession.UserID.ToString();
            entity.COLLECT_USER = _userSession.RealName;
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

        private async Task BeforeUpdate(SP_COLLECT entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserName;
            entity.MODIFYDATE = dt;

        }

        private async Task BeforeDelete(SP_COLLECT entity)
        {
            await _dbContext.DeleteAsync<SP_COLLECT_DET>(x => x.COLLECT_ID == entity.COLLECT_ID);
            await _dbContext.DeleteAsync<SP_COLLECT_REQUEST>(x => x.COLLECT_ID == entity.COLLECT_ID);
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        public async Task<int> Submit(List<string> sids)
        {
            var updatedevice = await _dbContext.UpdateAsync<SP_COLLECT>(x => sids.Contains(x.COLLECT_ID),
                    x => new SP_COLLECT
                    {
                        AUDITING = "1"
                    });
            var appledetId = _dbContext.Query<SP_COLLECT_REQUEST>().Where(t => sids.Contains(t.COLLECT_ID)).Select(t => t.REQUEST_DET_ID).ToList();
            await _dbContext.UpdateAsync<SP_APPLY_DETAIL>(x => appledetId.Contains(x.SPDET_ID),
                  x => new SP_APPLY_DETAIL
                  {
                      SP_STATUS = "40"//采购中
                  });

            var list = _dbContext.Query<SP_COLLECT>().Where(x => sids.Contains(x.COLLECT_ID)).ToList();

            if (list.Count > 0)
            {
                DateTime? dt = await _dbContext.GetSysdate();
                var importDetail = new List<SP_ORDER_DETAIL>();
                var importList = new List<SP_ORDER>();
                string type = $"DD{dt.Value.ToString("yyyyMM")}";
                string def = type + "0000";
                var model = await _dbContext.Query<SP_ORDER>(x => x.ORDER_CODE.Contains(type)).Select(x => Sql.Max(x.ORDER_CODE) ?? def).FirstOrDefaultAsync();
                var i = 1;
 
                foreach (var item in list)
                {
                    var index = model.SubStr(8, 4).CastTo<int>() + i;
                    //形成物资询价方案
                    var temp = new SP_ORDER
                    {
                        PURPLAN_ID = item.COLLECT_ID,
                        ORDER_ID = GuidHelper.NewSnowflakeId().ToString(),
                        ORDER_CODE = type + index.ToString("D4"),
                        ORDER_DATE = dt,
                        ORDER_MONEY = item.COLLECT_PRICE,
                        BUY_USERID = item.COLLECT_USERID,
                        BUY_USER = item.COLLECT_USER,
                        PROVIDER_ID= item.PROVIDER_ID,
                        PROVIDER_NAME = item.PROVIDER_NAME,
                        CREATE_USERID = _userSession.UserID.ToString(),
                        CREATEDATE = dt,
                        MODIFY_USERID = _userSession.UserID.ToString(),
                        MODIFYDATE = dt,
                        AUDITING = "0",
                        IS_STOP = "0"
                    };
                    i++;
                    importList.Add(temp);
                    await Task.CompletedTask;

                    var data = _dbContext.Query<SP_COLLECT_REQUEST>().Where(x => x.COLLECT_ID == item.COLLECT_ID).ToList();
                    foreach (var det in data)
                    {
                        var apply = _dbContext.Query<SP_APPLY>()
                            .LeftJoin<SP_APPLY_DETAIL>((a,b)=>a.APPLY_ID == b.APPLY_ID)
                            .Where((a, b) =>b.SPDET_ID == det.REQUEST_DET_ID)
                            .Select((a, b) => new { 
                             a.APPLY_NO,
                             a.USE_MEMO
                            })
                            .First();
                        var req = det.MapTo<SP_ORDER_DETAIL>();
                        req.APPLY_NO = apply?.APPLY_NO;
                        req.USE_MEMO = apply?.USE_MEMO;
                        req.APPLY_USERID = det.REQUEST_USERID;
                        req.APPLY_USER = det.REQUEST_USER;
                        req.SPDET_ID = det.REQUEST_DET_ID;

                        req.ORDERDET_ID = GuidHelper.NewSnowflakeId().ToString();
                        req.CREATE_USERID = _userSession.UserID.ToString();
                        req.CREATEDATE = dt;
                        req.MODIFY_USERID = _userSession.UserID.ToString();
                        req.MODIFYDATE = dt;

                        req.COUNT = det.CHECK_NUM;
                        req.PRICE = det.TAX_PRICE;
                        req.MONEY = det.COLLECT_MONEY;
                        req.ORDER_ID = temp.ORDER_ID;
                        importDetail.Add(req);
                        await Task.CompletedTask;
                    }
                }

                await _dbContext.InsertRangeAsync<SP_ORDER>(importList);
                await _dbContext.InsertRangeAsync<SP_ORDER_DETAIL>(importDetail);
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
            return await _dbContext.Query<SP_COLLECT_DET>().GetGridData(request);
        }

        /// <summary>
        /// 明细-保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> DetailSave(SaveRequest<SP_COLLECT_DET> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.SP_CODE,
                    c.SP_NAME,
                    c.SP_TYPE,
                    c.OTHER_CODE,
                    c.BRAND,
                    c.UNIT,
                    c.FACTORY,
                    c.COLLECT_NUM,
                    c.MEMO,
                    c.ARRIVE_NUM,
                    c.COLLECT_DET_ID,
                    c.COLLECT_ID,
                    c.SP_ID,
                    c.REQUEST_DET_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.IN_NUM,
                    c.TYPE_CODE,
                    c.TYPE_NAME,
                    c.TYPE_ID,
                    c.SP_DAIMA,
                    c.SP_TUHAO,
                    c.SP_ENGNAME,
                    c.STORE_NUM
                },
                c => a => a.COLLECT_DET_ID == c.COLLECT_DET_ID, BeforeAddDet, BeforeUpdateDet);
        }

        private async Task BeforeAddDet(SP_COLLECT_DET entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.COLLECT_DET_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }

        private async Task BeforeUpdateDet(SP_COLLECT_DET entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;

        }

        /// <summary>
        /// 需求列表
        /// </summary>
        /// <param name="COLLECT_DET_ID"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> RequestListAsync(string COLLECT_DET_ID,GridRequest request)
        {
            return await _dbContext.Query<SP_COLLECT_REQUEST>().Where(t => t.COLLECT_DET_ID == COLLECT_DET_ID).GetGridData(request);
        }

        /// <summary>
        /// 需求保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> RequestSave(SaveRequest<SP_COLLECT_REQUEST> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.REQUEST_CODE,
                    c.REQUEST_USER,
                    c.REQUEST_USERID,
                    c.SP_CODE,
                    c.SP_NAME,
                    c.SP_TYPE,
                    c.BRAND,
                    c.OTHER_CODE,
                    c.UNIT,
                    c.FACTORY,
                    c.DEPT_NAME,
                    c.DEPT_ID,
                    c.SEC_DEPT,
                    c.SEC_DEPTID,
                    c.MEMO,
                    c.COLLECT_REQUEST_ID,
                    c.COLLECT_DET_ID,
                    c.COLLECT_ID,
                    c.REQUEST_DET_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.TYPE_CODE,
                    c.TYPE_NAME,
                    c.TYPE_ID,
                    c.SP_DAIMA,
                    c.SP_TUHAO,
                    c.SP_ENGNAME,
                    c.SP_ID,
                    c.CONFIRM_NUM,
                    c.COLLECT_MONEY,
                    c.TAX_PRICE,
                    c.TAX_MONEY,
                    c.NOTAX_PRICE,
                    c.NOTAX_MONEY,
                    c.REQUEST_NUM,
                    c.CHECK_NUM,
                    c.IS_FULLBUY
                },
                c => a => a.COLLECT_REQUEST_ID == c.COLLECT_REQUEST_ID, BeforeAddRequest, BeforeUpdateRequest, BeforeDeleteRequest, false, null, AfterSaveRequest);
        }
        private async Task BeforeAddRequest(SP_COLLECT_REQUEST entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.COLLECT_REQUEST_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }
        private async Task BeforeUpdateRequest(SP_COLLECT_REQUEST entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;

        }
        private async Task BeforeDeleteRequest(SP_COLLECT_REQUEST entity)
        {
            var data = _dbContext.Query<SP_COLLECT_REQUEST>().Where(t => t.COLLECT_DET_ID == entity.COLLECT_DET_ID).Count();
            if (data == 1)
            {
                _dbContext.DeleteByKey<SP_COLLECT_DET>(entity.COLLECT_DET_ID);
            }
            else
            {
                await _dbContext.UpdateAsync<SP_COLLECT_DET>(x => x.COLLECT_DET_ID == entity.COLLECT_DET_ID,
                    x => new SP_COLLECT_DET
                    {
                        COLLECT_NUM = x.COLLECT_NUM - entity.CHECK_NUM
                    });
            }

            await _dbContext.UpdateAsync<SP_APPLY_DETAIL>(x => x.SPDET_ID== entity.REQUEST_DET_ID,
                 x => new SP_APPLY_DETAIL
                 {
                     SP_STATUS = "20"//请购中
                 });
        }
        private async Task AfterSaveRequest(List<SP_COLLECT_REQUEST> added, List<SP_COLLECT_REQUEST> updated, List<SP_COLLECT_REQUEST> deleted)
        {
            var applyId = added.Count == 0 ? (updated.Count == 0 ? deleted.Select(c => c.COLLECT_ID).FirstOrDefault() : updated.Select(c => c.COLLECT_ID).FirstOrDefault()) : added.Select(c => c.COLLECT_ID).FirstOrDefault();
            await Task.CompletedTask;
            if (!string.IsNullOrEmpty(applyId))
            {
                var data = _dbContext.Query<SP_COLLECT_REQUEST>().Where(t => t.COLLECT_ID == applyId)
                    .Select(t => new
                    {
                        t.COLLECT_DET_ID,
                        t.COLLECT_MONEY,
                        t.TAX_MONEY,
                        t.NOTAX_MONEY,
                        t.CHECK_NUM
                    }).ToList();

                var COLLECT_PRICE = data.Sum(t => t.COLLECT_MONEY) ?? 0;
                var TAX_MONEY = data.Sum(t => t.TAX_MONEY) ?? 0;
                var NOTAX_MONEY = data.Sum(t => t.NOTAX_MONEY) ?? 0;
                await _dbContext.UpdateAsync<SP_COLLECT>(x => x.COLLECT_ID == applyId,
                    x => new SP_COLLECT
                    {
                        COLLECT_PRICE = COLLECT_PRICE,
                        TAX_MONEY = TAX_MONEY,
                        NOTAX_MONEY = NOTAX_MONEY
                    });

                var det = data.GroupBy(x => x.COLLECT_DET_ID)
                    .Select(x => new
                    {
                        x.Key,
                        CHECK_NUM = x.Sum(x => x.CHECK_NUM)
                    });

                foreach (var sp in det)
                {
                    await _dbContext.UpdateAsync<SP_COLLECT_DET>(x => x.COLLECT_DET_ID == sp.Key,
                   x => new SP_COLLECT_DET
                   {
                       COLLECT_NUM = sp.CHECK_NUM
                   });
                }
            }
        }

        /// <summary>
        /// 待请购的采购申请明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> SpApplyListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_APPLY_DETAIL>()
                .LeftJoin<SP_APPLY>((a, b) => a.APPLY_ID == b.APPLY_ID)
                  .Where((a, b) => a.SP_STATUS == "20" && b.AUDITING == "1")
                .Select((a, b) => new
                {
                    a.SPDET_ID,
                    a.SP_ID,
                    b.APPLY_NO,
                    a.SP_CODE,
                    a.SP_NAME,
                    a.SP_SIZE,
                    a.PRODUCE,
                    a.UNIT,
                    a.TYPE_NAME
                })
                .GetGridData(request);
        }

        class SpApplyDetRes : SP_APPLY_DETAIL
        {
            /// <summary>
            /// 申请编号
            /// </summary>
            public string APPLY_NO;

            public string APPLY_USER;
            public string APPLY_USERID;
            public string DEPT_NAME { get; set; }

            /// <summary>
            /// 申请部门ID
            /// </summary>
            public string DEPT_ID { get; set; }

            /// <summary>
            /// 二级单位
            /// </summary>
            public string SEC_DEPT { get; set; }

            /// <summary>
            /// 二级单位ID
            /// </summary>
            public string SEC_DEPTID { get; set; }
        }
        /// <summary>
        /// 选中的采购申请明细
        /// </summary>
        /// <param name="SpdetID"></param>
        /// <param name="Cid"></param>
        /// <returns></returns>
        public async Task<int> SelectApply(List<string> SpdetID, string Cid)
        {
            var appledet = _dbContext.Query<SP_APPLY_DETAIL>()
                .Where(t => SpdetID.Contains(t.SPDET_ID))
                .LeftJoin<SP_APPLY>((a, b) => a.APPLY_ID == b.APPLY_ID)
                .Select((a, b) => new SpApplyDetRes
                {
                    SPDET_ID = a.SPDET_ID,
                    APPLY_ID = a.APPLY_ID,
                    SP_ID = a.SP_ID,
                    SP_CODE = a.SP_CODE,
                    SP_NAME = a.SP_NAME,
                    SP_SIZE = a.SP_SIZE,
                    PRODUCE = a.PRODUCE,
                    UNIT = a.UNIT,
                    COUNT = a.COUNT,
                    STORE_NUM = a.STORE_NUM,
                    YG_PRICE = a.YG_PRICE,
                    YG_MONEY = a.YG_MONEY,
                    LAST_PROVIDERID = a.LAST_PROVIDERID,
                    LAST_PROVIDER = a.LAST_PROVIDER,
                    TYPE_ID = a.TYPE_ID,
                    TYPE_CODE = a.TYPE_CODE,
                    TYPE_NAME = a.TYPE_NAME,
                    IS_STOP = a.IS_STOP,
                    MEMO = a.MEMO,
                    IS_GEN = a.IS_GEN,
                    TENANT_ID = a.TENANT_ID,
                    PURTYPE_ID = a.PURTYPE_ID,
                    PURTYPE_NAME = a.PURTYPE_NAME,
                    IS_CANCEL = a.IS_CANCEL,
                    NO_PRODUCE = a.NO_PRODUCE,
                    COMP_CODE = a.COMP_CODE,
                    STORE_MONTH = a.STORE_MONTH,
                    PUR_PERIOD = a.PUR_PERIOD,
                    ONROAD_NUM = a.ONROAD_NUM,
                    PRO_ID = a.PRO_ID,
                    PRO_DET_ID = a.PRO_DET_ID,
                    PERIOD = a.PERIOD,
                    IS_XY = a.IS_XY,
                    WARRANTY = a.WARRANTY,
                    DELIVERY_CODE = a.DELIVERY_CODE,
                    XHZQ = a.XHZQ,
                    SYDD = a.SYDD,
                    CGFS = a.CGFS,
                    SYDDDEPTID = a.SYDDDEPTID,
                    COUNT2 = a.COUNT2,
                    CGFS2 = a.CGFS2,
                    SP_CODE2 = a.SP_CODE2,
                    COMP_CODE2 = a.COMP_CODE2,
                    SP_NAME2 = a.SP_NAME2,
                    SYDD2 = a.SYDD2,
                    SYDDDEPTID2 = a.SYDDDEPTID2,
                    SP_SIZE2 = a.SP_SIZE2,
                    PRODUCE2 = a.PRODUCE2,
                    UNIT2 = a.UNIT2,
                    ZKCS = a.ZKCS,
                    QYKCSL = a.QYKCSL,
                    APPLY_NO = b.APPLY_NO,
                    APPLY_USER = b.APPLY_USER,
                    DEPT_NAME = b.DEPT_NAME,
                    DEPT_ID = b.DEPT_ID,
                    SEC_DEPT = b.SEC_DEPT,
                    SEC_DEPTID= b.SEC_DEPTID,
                    APPLY_USERID = b.APPLY_USERID
                }).ToList();

            var spIds = appledet.Select(t => t.SP_ID).Distinct().ToList();

            var importResult = new List<SP_COLLECT_DET>();

            var importRequest = new List<SP_COLLECT_REQUEST>();
            var colldet = _dbContext.Query<SP_COLLECT_DET>().Where(t => t.COLLECT_ID == Cid).ToList();
            foreach (var spId in spIds)
            {
                var COLLECT_DET_ID = colldet.Count > 0 ? colldet.Where(t => t.SP_ID == spId).Select(t => t.COLLECT_DET_ID).First() : "";
                var data = appledet.Where(t => t.SP_ID == spId).ToList();
                var det = data.First();
                if (string.IsNullOrEmpty(COLLECT_DET_ID))
                {
                    var temp = det.MapTo<SP_COLLECT_DET>();
                    await BeforeAddDet(temp);
                    temp.COLLECT_ID = Cid;
                    COLLECT_DET_ID = temp.COLLECT_DET_ID;
                    importResult.Add(temp);
                    await Task.CompletedTask;
                }

                foreach (var item in data)
                {
                    var req = item.MapTo<SP_COLLECT_REQUEST>();
                    await BeforeAddRequest(req);
                    req.COLLECT_ID = Cid;
                    req.COLLECT_DET_ID = COLLECT_DET_ID;
                    req.REQUEST_CODE = item.APPLY_NO;
                    req.REQUEST_DET_ID = item.SPDET_ID;
                    req.REQUEST_NUM = item.COUNT;
                    req.REQUEST_USER = item.APPLY_USER;
                    req.REQUEST_USERID = item.APPLY_USERID;
                    importRequest.Add(req);
                    await Task.CompletedTask;
                }
            }

            if (importResult.Count > 0)
            {
                await _dbContext.InsertRangeAsync<SP_COLLECT_REQUEST>(importRequest);
                await _dbContext.InsertRangeAsync<SP_COLLECT_DET>(importResult);
            }

            var appIds = appledet.Select(t => t.SPDET_ID).ToList();
            await _dbContext.UpdateAsync<SP_APPLY_DETAIL>(x => appIds.Contains(x.SPDET_ID),
                x => new SP_APPLY_DETAIL
                {
                    SP_STATUS = "30"//请购中
                });
            return appledet.Count;
        }
        #endregion
    }
}
