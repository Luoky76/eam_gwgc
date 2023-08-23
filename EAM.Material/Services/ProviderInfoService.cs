using EAM.Material.Interfaces;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Services
{
    public class ProviderInfoService : IProviderInfoService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;

        public ProviderInfoService(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userSession = userSession;
        }

        /// <summary>
        /// 根据ID获取单行记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<PROVIDER> GetAsync(string id)
        {
            var query = await _dbContext.Query<PROVIDER>().Where(c => c.PROVIDER_ID == id).FirstAsync();
            return query;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<PROVIDER>()
                .GetGridData(request);
            return list;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveAsync(SaveRequest<PROVIDER> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.PROVIDER_ID,
                    c.AUDITING,
                    c.ORG_NAME,
                    c.DEPT_NAME,
                    c.PROVIDER_CODE,
                    c.PROVIDER_NAME,
                    c.ABBREVIATION,
                    c.REG_ADDRESS,
                    c.LEGAL_REPRESENT,
                    c.ENTITY_REG_FUND,
                    c.ENTER_NATURE,
                    c.LICENSE_CODE,
                    c.ENTER_QUALITY,
                    c.BUSINESS_LIC,
                    c.PROVIDER_CONTACT,
                    c.PROVIDER_PHONE,
                    c.FAX,
                    c.EMAIL,
                    c.WEBSITE,
                    c.AVERAGE_TAX,
                    c.BIZ_SCOPE,
                    c.PROVIDER_SRC,
                    c.PROVIDER_TYPE,
                    c.PROVIDER_STATUS,
                    c.COOPERATION,
                    c.CONCLUSION,
                    c.QUA_CONCLUSION,
                    c.COUNTY,
                    c.PROVINCE,
                    c.CITY,
                    c.BANK_NAME,
                    c.BANK_CODE,
                    c.TAX_REG_NO,
                    c.QUALITY,
                    c.DELIVERY,
                    c.ORDER_SIT,
                    c.CREDIT,
                    c.STRENGTH,
                    c.RESPONE_SPEED,
                    c.SERVICE_QUAL,
                    c.ORG_ID,
                    c.DEPT_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                },
                c => a => a.PROVIDER_ID == c.PROVIDER_ID
                , BeforeAdd, null, null, false, null, null);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(PROVIDER entity)
        {
            if (entity.PROVIDER_ID.IsNullOrEmpty())
            {
                entity.PROVIDER_ID = GuidHelper.NewSnowflakeId().ToString();
            }

            //自动添加供应商编码
            if (entity.PROVIDER_CODE.IsNullOrEmpty())
            {
                //示例PR2023070001
                string headCode = "PR";
                var sysdate = await _dbContext.GetSysdate();
                string dateCode = sysdate.Value.ToString("yyyyMM");
                string newCode = headCode + dateCode + "0000";
                string model = await _dbContext.Query<PROVIDER>(a => a.PROVIDER_CODE.Contains(headCode + dateCode))
                    .Select(a => Sql.Max(a.PROVIDER_CODE) ?? newCode).FirstOrDefaultAsync();
                entity.PROVIDER_CODE = headCode + (long.Parse(model.Substring(headCode.Length)) + 1).ToString();
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(PROVIDER entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(PROVIDER entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 保存后验证
        /// </summary>
        private async Task AfterSave(List<PROVIDER> added, List<PROVIDER> updated, List<PROVIDER> deleted)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public async Task<AjaxResult> ComboxData()
        {
            try
            {
                var data = await _comboxDataService.Get(new Dictionary<string, object>(){
                    {"ProviderName", null},
                    {"Auditing", null }
                });

                return AjaxResult.Success(data);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }
    }
}
