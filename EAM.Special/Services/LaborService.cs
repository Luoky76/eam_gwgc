using Chloe;
using EAM.Special.Interfaces;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Grid;
using Gksyb.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gksyb.Common;
using Org.BouncyCastle.Utilities.Encoders;
using DocumentFormat.OpenXml.Drawing.Charts;
using Gksyb.Model.Core;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.CodeAnalysis;
using NPOI.OpenXmlFormats.Dml.Diagram;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using WkHtmlToPdfDotNet;
using NPOI.SS.Formula.Functions;

namespace EAM.Special.Services
{

    public class LaborService : ILaborService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly IUserService _userService;
        private readonly ICorpService _corpService;

        public LaborService(IDbContext dbContext, IComboxDataService comboxDataService, IUserService userService, ICorpService corpService)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userService = userService;
            _corpService = corpService;
        }

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public async Task<AjaxResult> ComboxData()
        {
            try
            {
                var data = await _comboxDataService.Get(new Dictionary<string, object>()
                {

                });
                data.TryAdd("Corp", await _corpService.ComboxDataAsync());

                return AjaxResult.Success(data);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }
        #region 劳保人员清单
        public async Task<GridData> laborUserListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<LABOR_USER>().GetGridData(request);
            return list;
        }

        public async Task<AjaxResult> SaveAsync(SaveRequest<LABOR_USER> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.USER_SID,
                    c.USER_NAME,
                    c.USER_ID,
                    c.USER_CODE,
                    c.SEX,
                    c.MODIFYDATE,
                    c.MODIFY_USERID,
                    c.IS_NOVALID,
                    c.DEPT_NAME,
                    c.DEPT_ID,
                    c.DEPT_CODE,
                    c.CREATEDATE,
                    c.CREATE_USERID,
                    c.BIRTHDAY,
                },
                c => a => a.USER_SID == c.USER_SID
                , BeforeAdd, null, null, false, null, null);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(LABOR_USER entity)
        {
            entity.USER_ID = GuidHelper.NewSnowflakeId().ToString();
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(LABOR_USER entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(LABOR_USER entity)
        {
            await Task.CompletedTask;
        }


        #endregion

        #region 劳保需求申请
        public async Task<GridData> laborrequestListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<LABOR_REQUEST>().GetGridData(request);
            return list;
        }

        public async Task<GridData> laborrequestdetListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<LABOR_REQUEST_DET>().GetGridData(request);
            return list;

        }
        public async Task<GridData> laborrequestListListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<LABOR_REQUEST_LIST>().GetGridData(request);
            return list;

        }

        public async Task<AjaxResult> SaveAsync(SaveRequest<LABOR_REQUEST> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.AUDITING,
                    c.REQUEST_CODE,
                    c.REQUEST_DATE,
                    c.REQUEST_MONTH,
                    c.REQUEST_YEAR,
                    c.REQUEST_USER,
                    c.REQUEST_USERID,
                    c.DEPT_CODE,
                    c.DEPT_NAME,
                    c.DEPT_ID,
                    c.SHIP_NAME,
                    c.SHIP_ID,
                    c.SHIP_CODE,
                    c.SEC_DEPT,
                    c.SEC_DEPTID,
                    c.MEMO,
                    c.REQUEST_TYPE,
                    c.FORM_ID,
                    c.REQUEST_SPTYPE,
                    c.SRC_CODE,
                    c.REQUEST_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                },
                c => a => a.REQUEST_ID == c.REQUEST_ID
                , BeforeAdd, null, null, false, null, null);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(LABOR_REQUEST entity)
        {
            entity.REQUEST_ID = GuidHelper.NewSnowflakeId().ToString();
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(LABOR_REQUEST entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(LABOR_REQUEST entity)
        {
            await Task.CompletedTask;
        }


        public async Task<AjaxResult> SaveAsync(SaveRequest<LABOR_REQUEST_DET> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.SP_STATUS,
                    c.SP_CODE,
                    c.SP_DAIMA,
                    c.SP_NAME,
                    c.SP_ENGNAME,
                    c.SP_TYPE,
                    c.SP_TUHAO,
                    c.OTHER_CODE,
                    c.BRAND,
                    c.UNIT,
                    c.FACTORY,
                    c.REQUEST_NUM,
                    c.CAN_OUT_NUM,
                    c.MEMO,
                    c.STOCK_ID,
                    c.TYPE_CODE,
                    c.STOCK_NAME,
                    c.STOCK_CODE,
                    c.TYPE_NAME,
                    c.TYPE_ID,
                    c.APPLY_USER,
                    c.APPLY_USERID,
                    c.APPLY_ID,
                    c.PURPOSE,
                    c.REQUEST_DET_ID,
                    c.REQUEST_ID,
                    c.SP_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.REQUEST_LIST_ID,
                    c.DEPT_CODE,
                    c.DEPT_NAME,
                    c.USER_CODE,
                    c.USER_NAME,
                },
                c => a => a.REQUEST_ID == c.REQUEST_ID
                , BeforeAdd, null, null, false, null, null);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(LABOR_REQUEST_DET entity)
        {
            entity.REQUEST_ID = GuidHelper.NewSnowflakeId().ToString();
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(LABOR_REQUEST_DET entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(LABOR_REQUEST_DET entity)
        {
            await Task.CompletedTask;
        }

        #endregion

        #region 劳保采购计划
        public async Task<GridData> laborcollectListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<LABOR_COLLECT>().GetGridData(request);
            return list;
        }

        public async Task<AjaxResult> SaveAsync(SaveRequest<LABOR_COLLECT> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.AUDITING,
                    c.COLLECT_CODE,
                    c.COLLECT_DATE,
                    c.COLLECT_USER,
                    c.COLLECT_USERID,
                    c.DEPT_NAME,
                    c.DEPT_ID,
                    c.COLLECT_METHOD,
                    c.MEMO,
                    c.COLLECT_PRICE,
                    c.RATIO,
                    c.TAX_MONEY,
                    c.NOTAX_MONEY,
                    c.PROVIDER_CODE,
                    c.PROVIDER_ID,
                    c.PROVIDER_NAME,
                    c.CONSULT_PROVIDER,
                    c.COLLECT_SPTYPE,
                    c.BD_NO,
                    c.COLLECT_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,

                },
                c => a => a.COLLECT_ID == c.COLLECT_ID
                , BeforeAdd, null, null, false, null, null);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(LABOR_COLLECT entity)
        {
            entity.COLLECT_ID = GuidHelper.NewSnowflakeId().ToString();
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(LABOR_COLLECT entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(LABOR_COLLECT entity)
        {
            await Task.CompletedTask;
        }


        #endregion
    }
}
