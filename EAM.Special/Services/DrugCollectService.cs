using Chloe;
using EAM.Special.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Special.Services
{
    public class DrugCollectService : IDrugCollectService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;

        public DrugCollectService(IDbContext dbContext, IComboxDataService comboxDataService)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<DRUG_COLLECT>().Select(c => new
            {
                c.COLLECT_ID,
                c.AUDITING,
                c.CONFIRM_AUDIT,
                c.COLLECT_CODE,
                c.CONFIRM_CODE,
                c.CONFIRM_DATE,
                c.DEPT_ID,
                c.DEPT_NAME,
                c.SEC_DEPTID,
                c.SEC_DEPT,
                c.HOUSE_ID,
                c.HOUSE_NAME,
                c.COLLECT_METHOD,
                c.MEMO,
                c.COLLECT_PRICE,
                c.RATIO,
                c.TAX_MONEY,
                c.NOTAX_MONEY,
                c.PROVIDER_ID,
                c.PROVIDER_NAME,
                c.PROVIDER_CODE,
                c.CONSULT_PROVIDER,
                c.HOUSE_CODE,
                c.STORE_TYPE,
                c.HOUSE_USER,
                c.HOUSE_USERID,
                c.COLLECT_SPTYPE,
                c.BD_NO,
                c.CREATE_USERID,
                c.CREATEDATE,
                c.MODIFY_USERID,
                c.MODIFYDATE
            }).GetGridData(request);
            return list;
        }

        /// <summary>
        /// 根据ID获取单行记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DRUG_COLLECT> GetAsync(string id)
        {
            var query = await _dbContext.Query<DRUG_COLLECT>().Where(c => c.COLLECT_ID == id).FirstAsync();
            return query;
        }

        /// <summary>
        /// 生成主键
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        public string CreatePrimaryKey()
        {
            return GuidHelper.NewSnowflakeId().ToString();
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveAsync(SaveRequest<DRUG_COLLECT> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.COLLECT_ID,
                    c.AUDITING,
                    c.CONFIRM_AUDIT,
                    c.COLLECT_CODE,
                    c.CONFIRM_CODE,
                    c.CONFIRM_DATE,
                    c.DEPT_ID,
                    c.DEPT_NAME,
                    c.SEC_DEPTID,
                    c.SEC_DEPT,
                    c.HOUSE_ID,
                    c.HOUSE_NAME,
                    c.COLLECT_METHOD,
                    c.MEMO,
                    c.COLLECT_PRICE,
                    c.RATIO,
                    c.TAX_MONEY,
                    c.NOTAX_MONEY,
                    c.PROVIDER_ID,
                    c.PROVIDER_NAME,
                    c.PROVIDER_CODE,
                    c.CONSULT_PROVIDER,
                    c.HOUSE_CODE,
                    c.STORE_TYPE,
                    c.HOUSE_USER,
                    c.HOUSE_USERID,
                    c.COLLECT_SPTYPE,
                    c.BD_NO,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                },
                c => a => a.COLLECT_ID == c.COLLECT_ID
                , BeforeAdd, null, null, false, null, null);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(DRUG_COLLECT entity)
        {
            if (entity.COLLECT_ID.IsNullOrEmpty())
            {
                entity.COLLECT_ID = GuidHelper.NewSnowflakeId().ToString();
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(DRUG_COLLECT entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(DRUG_COLLECT entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 保存后验证
        /// </summary>
        private async Task AfterSave(List<DRUG_COLLECT> added, List<DRUG_COLLECT> updated, List<DRUG_COLLECT> deleted)
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
                var data = await _comboxDataService.Get(new Dictionary<string, object>()
                {

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
