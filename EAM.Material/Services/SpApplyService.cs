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
using NPOI.OpenXmlFormats.Dml.Diagram;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
        /// 获取单行数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<AjaxResult> GetAsync(string id)
        {
            var row = await _dbContext.Query<SP_APPLY>().Where(c => c.APPLY_ID == id).FirstAsync();
            return AjaxResult.Success(row);
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
            entity.APPLY_NO = $"SQ{dt.Value.ToString("yyyyMMddHHmmss")}";

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

            entity.MODIFY_USERID = _userSession.UserName;
            entity.MODIFYDATE = dt;

        }


        public async Task<int> Submit(List<string> sids)
        {
            var updatedevice = await _dbContext.UpdateAsync<SP_APPLY>(x => sids.Contains(x.APPLY_ID),
                    x => new SP_APPLY
                    {
                        AUDITING = "1"
                    });
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
        }

        private async Task BeforeUpdateDet(SP_APPLY_DETAIL entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;

        }

        private async Task AfterSaveDet(List<SP_APPLY_DETAIL> added, List<SP_APPLY_DETAIL> updated, List<SP_APPLY_DETAIL> deleted)
        {
            var applyId = added == null? updated.Select(c => c.APPLY_ID).FirstOrDefault():added.Select(c => c.APPLY_ID).FirstOrDefault();
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
    }
}
