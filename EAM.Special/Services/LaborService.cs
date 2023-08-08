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


    }
}
