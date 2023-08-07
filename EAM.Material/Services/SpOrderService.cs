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
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_ORDER>().GetGridData(request);
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
                c => a => a.ORDER_ID == c.ORDER_ID, BeforeAdd, BeforeUpdate);
        }

        private async Task BeforeAdd(SP_ORDER entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.ORDER_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }

        private async Task BeforeUpdate(SP_ORDER entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }

        public async Task<int> Submit(List<string> sids)
        {
            var updatedevice = await _dbContext.UpdateAsync<SP_ORDER>(x => sids.Contains(x.ORDER_ID),
                    x => new SP_ORDER
                    {
                        AUDITING = "1",
                        BUY_USER = _userSession.RealName,
                        BUY_USERID = _userSession.UserID.ToString(),
                        BUY_USERDEPTID= _userSession.Corp.CorpID
                    });
            return updatedevice;
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
        }

        private async Task DetBeforeUpdate(SP_ORDER_DETAIL entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }
    }
}
