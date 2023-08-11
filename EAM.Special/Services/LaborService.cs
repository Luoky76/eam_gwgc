using Chloe;
using DocumentFormat.OpenXml.Wordprocessing;
using EAM.Special.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.CodeAnalysis;
using NPOI.OpenXmlFormats.Dml.Diagram;
using System;
using WkHtmlToPdfDotNet;

namespace EAM.Special.Services
{

    public class LaborService : ILaborService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly IUserService _userService;
        private readonly ICorpService _corpService;
        private readonly UserSession _userSession;
        private string _rentID = string.Empty, errMsg = string.Empty;

        public LaborService(IDbContext dbContext, IComboxDataService comboxDataService, IUserService userService, ICorpService corpService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userService = userService;
            _corpService = corpService;
            _userSession = userSession;
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
                    { "Auditing", null },
                    { "User", null },
                    { "RentState", null }
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


        #region 劳保用品退换
        public async Task<GridData> LaborExchangeListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<LABOR_EXCHANGE>().GetGridData(request);
            return list;
        }
        public async Task<GridData> GetLaborExchangeAppDetList(string id)
        {
            var result = await _dbContext.Query<LABOR_EXCHANGE_APPDET>(x => x.EXCHANGE_ID.Equals(id)).ToListAsync();
            GridData data = new GridData
            {
                Rows = result,
                Total = result.Count
            };
            return data;
        }
        public async Task<AjaxResult> LaborExchangeSave(SaveRequest<LABOR_EXCHANGE> request, SaveRequest<LABOR_EXCHANGE_APPDET> requestdet)
        {
            //从表保存的主表ID通过公共变量 _rendID 来传递给从表

            using (var trans = _dbContext.BeginTransaction())  //事务保证保存数据的一致性
            {
                bool mainSuccess = false, detSuccess = false;
                var execResult = await _dbContext.SaveEntityAnsyc(request,
                     c => new
                     {
                         c.AUDITING,
                         c.EXCHANGE_CODE,
                         c.EXCHANGE_DATE,
                         c.EXCHANGE_TYPE,
                         c.EXCHANGE_USER,
                         c.EXCHANGE_DEPT,
                         c.MEMO,
                         c.EXCHANGE_ID,
                         c.EXCHANGE_USERID,
                         c.EXCHANGE_DEPTID,
                         c.AUDIT_USERID,
                         c.AUDIT_DEPTID,
                         c.CREATE_USERID,
                         c.CREATEDATE,
                         c.MODIFY_USERID,
                         c.MODIFYDATE,
                         c.EXCHANGE_REASON,
                     },
                     c => a => a.EXCHANGE_ID == c.EXCHANGE_ID
                     , LaborExchangeBeforAdd, LaborExchangeBeforUpdate, LaborExchangeBeforDelete, false, null, null);

                mainSuccess = !execResult.IsError;
                if (mainSuccess)  //主表是否保存成功
                {
                    requestdet = requestdet ?? new SaveRequest<LABOR_EXCHANGE_APPDET>();

                    execResult = await _dbContext.SaveEntityAnsyc(requestdet,
                         c => new
                         {
                             c.SP_CODE,
                             c.SP_DAIMA,
                             c.SP_NAME,
                             c.SP_TYPE,
                             c.BRAND,
                             c.UNIT,
                             c.FACTORY,
                             c.OTHER_CODE,
                             c.EXCHANGE_NUM,
                             c.TYPE_CODE,
                             c.TYPE_NAME,
                             c.PURPOSE,
                             c.MEMO,
                             c.EXCHANGE_APPDET_ID,
                             c.EXCHANGE_ID,
                             c.TYPE_ID,
                             c.SP_ID,
                             c.CREATE_USERID,
                             c.CREATEDATE,
                             c.MODIFY_USERID,
                             c.MODIFYDATE,
                             c.STORE_ID,
                             c.OUT_DET_ID
                         },
                         c => a => a.EXCHANGE_APPDET_ID == c.EXCHANGE_APPDET_ID
                         , LaborExchangeAppDetBeforAdd, LaborExchangeAppDetBeforUpdate, null, false, null, null);

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
        private async Task LaborExchangeBeforAdd(LABOR_EXCHANGE entity)
        {
            var sysDate = await _dbContext.GetSysdate();

            string rentCode = "LBZJ" + sysDate.Value.ToString("yyyyMM");
            string sn = "0001";
            var lastCode = await _dbContext.Query<LABOR_EXCHANGE>(x => x.EXCHANGE_CODE.Contains(rentCode)).Select(x => Sql.Max(x.EXCHANGE_CODE)).FirstOrDefaultAsync();
            if (string.IsNullOrWhiteSpace(lastCode)) rentCode += sn;
            else rentCode += (int.Parse(lastCode.Substring(10, 4)) + 1).ToString("0000");

            entity.EXCHANGE_ID = _rentID = GuidHelper.NewSnowflakeId().ToString();
            entity.AUDITING = "0";
            entity.EXCHANGE_CODE = rentCode;
            entity.EXCHANGE_USERID = _userSession.UserID.ToString();
            entity.EXCHANGE_USER = _userSession.RealName;
            entity.EXCHANGE_DEPTID = _userSession.Corp.CorpID;
            entity.EXCHANGE_DEPT = _userSession.Corp.CName;
            entity.EXCHANGE_TYPE = "0";
            entity.CREATE_USERID = entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = entity.MODIFYDATE = sysDate;
        }
        private async Task LaborExchangeBeforUpdate(LABOR_EXCHANGE entity)
        {
            if (entity.AUDITING.Equals("0"))
            {
                var sysDate = await _dbContext.GetSysdate();
                _rentID = entity.EXCHANGE_ID;
                entity.AUDITING = "1";
                entity.MODIFY_USERID = _userSession.UserID.ToString();
                entity.MODIFYDATE = sysDate;
            }
            else
            {
                errMsg = "未提交的状态下才能修改";
                throw new MessageException("未提交的状态下才能修改");
            }
        }
        private async Task LaborExchangeBeforDelete(LABOR_EXCHANGE entity)
        {
            if (entity.AUDITING.Equals("0"))
                await _dbContext.DeleteAsync<LABOR_RENT_DET>(x => x.RENT_ID.Equals(entity.EXCHANGE_ID));
            else
            {
                errMsg = "未提交的状态下才能删除";
                throw new MessageException("未提交的状态下才能删除");
            }
        }
        private async Task LaborExchangeAppDetBeforAdd(LABOR_EXCHANGE_APPDET entity)
        {
            var sysDate = await _dbContext.GetSysdate();
            entity.EXCHANGE_APPDET_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.EXCHANGE_ID = _rentID;
            entity.CREATE_USERID = entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = entity.MODIFYDATE = sysDate;
        }
        private async Task LaborExchangeAppDetBeforUpdate(LABOR_EXCHANGE_APPDET entity)
        {
            var sysDate = await _dbContext.GetSysdate();
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = sysDate;
        }

        public async Task<AjaxResult> LaboExchangeGet(string id)
        {
            var mainData = await _dbContext.QueryByKeyAsync<LABOR_EXCHANGE>(id);
            var detData = await _dbContext.Query<LABOR_EXCHANGE_APPDET>(x => x.EXCHANGE_ID.Equals(id)).ToListAsync();
            var result = new
            {
                maindata = mainData,
                detdata = new GridData { Rows = detData, Total = detData.Count }
            };
            return AjaxResult.Success(result);
        }

        #endregion

        #region 劳保用品租借
        public async Task<GridData> LaborRentList(GridRequest request)
        {
            return await _dbContext.Query<LABOR_RENT>().GetGridData(request);
        }
        public async Task<GridData> GetLaborRentDetList(string rentId)
        {
            var result = await _dbContext.Query<LABOR_RENT_DET>(x => x.RENT_ID.Equals(rentId)).ToListAsync();
            GridData data = new GridData
            {
                Rows = result,
                Total = result.Count
            };
            return data;
        }
        public async Task<AjaxResult> LaborRentGet(string rentId)
        {
            var mainData = await _dbContext.QueryByKeyAsync<LABOR_RENT>(rentId);
            var detData = await _dbContext.Query<LABOR_RENT_DET>(x => x.RENT_ID.Equals(rentId)).ToListAsync();
            var result = new
            {
                maindata = mainData,
                detdata = new GridData { Rows = detData, Total = detData.Count }
            };
            return AjaxResult.Success(result);
        }
        public async Task<GridData> LaborStoreList(GridRequest request)
        {
            var result = await _dbContext.Query<SP_STORE>().GetGridData(request);
            return result;
        }
        public async Task<AjaxResult> LaborRentSave(SaveRequest<LABOR_RENT> request, SaveRequest<LABOR_RENT_DET> requestdet)
        {
            //从表保存的主表ID通过公共变量 _rendID 来传递给从表

            using (var trans = _dbContext.BeginTransaction())  //事务保证保存数据的一致性
            {
                bool mainSuccess = false, detSuccess = false;
                var execResult = await _dbContext.SaveEntityAnsyc(request,
                     c => new
                     {
                         c.AUDITING,
                         c.RENT_CODE,
                         c.RENT_DATE,
                         c.RENT_DEPT,
                         c.RENT_USER,
                         c.DEPT_NAME,
                         c.USER_NAME,
                         c.BEGIN_DATE,
                         c.END_DATE,
                         c.RENT_REASON,
                         c.MEMO,
                         c.RENT_ID,
                         c.RENT_DEPTID,
                         c.RENT_USERID,
                         c.DEPT_ID,
                         c.USER_ID,
                         c.EXPEND_DATE,
                         c.RENT_STATUS
                     },
                     c => a => a.RENT_ID == c.RENT_ID
                     , LaborRentBeforAdd, LaborRentBeforUpdate, LaborRentBeforDelete, false, null, null);

                mainSuccess = !execResult.IsError;
                if (mainSuccess)  //主表是否保存成功
                {
                    requestdet = requestdet ?? new SaveRequest<LABOR_RENT_DET>();

                    execResult = await _dbContext.SaveEntityAnsyc(requestdet,
                         c => new
                         {
                             c.SP_CODE,
                             c.SP_DAIMA,
                             c.SP_NAME,
                             c.SP_TYPE,
                             c.BRAND,
                             c.UNIT,
                             c.FACTORY,
                             c.OTHER_CODE,
                             c.RENT_NUM,
                             c.TYPE_CODE,
                             c.TYPE_NAME,
                             c.MEMO,
                             c.RENT_DET_ID,
                             c.RENT_ID,
                             c.TYPE_ID,
                             c.SP_ID,
                             c.STORE_ID,
                             c.HOUSE_ID
                         },
                         c => a => a.RENT_DET_ID == c.RENT_DET_ID
                         , LaborRentDetBeforAdd, LaborRentDetBeforUpdate, null, false, null, null);

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
        private async Task LaborRentBeforAdd(LABOR_RENT entity)
        {
            var sysDate = await _dbContext.GetSysdate();

            string rentCode = "LBZJ" + sysDate.Value.ToString("yyyyMM");
            string sn = "0001";
            var lastCode = await _dbContext.Query<LABOR_RENT>(x => x.RENT_CODE.Contains(rentCode)).Select(x => Sql.Max(x.RENT_CODE)).FirstOrDefaultAsync();
            if (string.IsNullOrWhiteSpace(lastCode)) rentCode += sn;
            else rentCode += (int.Parse(lastCode.Substring(10, 4)) + 1).ToString("0000");

            entity.RENT_ID = _rentID = GuidHelper.NewSnowflakeId().ToString();
            entity.AUDITING = "0";
            entity.RENT_CODE = rentCode;
            entity.USER_ID = _userSession.UserID.ToString();
            entity.USER_NAME = _userSession.RealName;
            entity.DEPT_ID = _userSession.Corp.CorpID;
            entity.DEPT_NAME = _userSession.Corp.CName;
            entity.RENT_STATUS = "0";
            entity.CREATE_USERID = entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = entity.MODIFYDATE = sysDate;
        }
        private async Task LaborRentBeforUpdate(LABOR_RENT entity)
        {
            var model = await _dbContext.QueryByKeyAsync<LABOR_RENT>(entity.RENT_ID);
            if (model.AUDITING.Equals("0"))
            {
                var sysDate = await _dbContext.GetSysdate();
                _rentID = entity.RENT_ID;
                entity.MODIFY_USERID = _userSession.UserID.ToString();
                entity.MODIFYDATE = sysDate;
            }
            else
            {
                errMsg = "未提交的状态下才能修改";
                throw new MessageException("未提交的状态下才能修改");
            }
        }
        private async Task LaborRentBeforDelete(LABOR_RENT entity)
        {
            if (entity.AUDITING.Equals("0"))
                await _dbContext.DeleteAsync<LABOR_RENT_DET>(x => x.RENT_ID.Equals(entity.RENT_ID));
            else
            {
                errMsg = "未提交的状态下才能删除";
                throw new MessageException("未提交的状态下才能删除");
            }
        }
        private async Task LaborRentDetBeforAdd(LABOR_RENT_DET entity)
        {
            var sysDate = await _dbContext.GetSysdate();
            entity.RENT_DET_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.RENT_ID = _rentID;
            entity.CREATE_USERID = entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = entity.MODIFYDATE = sysDate;
        }
        private async Task LaborRentDetBeforUpdate(LABOR_RENT_DET entity)
        {
            var sysDate = await _dbContext.GetSysdate();
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = sysDate;
        }
        #endregion
    }
}
