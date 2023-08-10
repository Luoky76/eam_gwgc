using Chloe;
using EAM.Special.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Special.Services
{

    public class LaborService : ILaborService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly IUserService _userService;
        private readonly ICorpService _corpService;
        private readonly UserSession _userSession;

        private string _rentID = string.Empty;

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
                    { "User", null }
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
        public async Task<AjaxResult> LaborRentSave(SaveRequest<LABOR_RENT> request, SaveRequest<LABOR_RENT_DET> requestdet)
        {
            //从表保存的主表ID通过公共变量 _rendID 来传递给从表

            using (var trans = _dbContext.BeginTransaction())  //事务保证保存数据的一致性
            {
                bool mainSuccess = true, detSuccess = true;
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
                     , LaborRentBeforAdd, LaborRentBeforUpdate, null, false, null, null);

                if (execResult.IsError) mainSuccess = false;  //主表是否保存成功

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

                if (execResult.IsError) detSuccess = false;  //明细表是否保存成功

                if (mainSuccess && detSuccess)
                    trans.Commit();
                else
                {
                    trans.Rollback();
                    return AjaxResult.Error("保存失败");
                }
            }
            return AjaxResult.Success("保存成功");
        }
        private async Task LaborRentBeforAdd(LABOR_RENT entity)
        {
            var sysDate = await _dbContext.GetSysdate();
            entity.RENT_ID = _rentID = GuidHelper.NewSnowflakeId().ToString();
            entity.CREATE_USERID = entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = entity.MODIFYDATE = sysDate;
        }
        private async Task LaborRentBeforUpdate(LABOR_RENT entity)
        {
            var sysDate = await _dbContext.GetSysdate();
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = sysDate;
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
