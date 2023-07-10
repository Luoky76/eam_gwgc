using EAM.Material.Interfaces;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Services
{
    public class ProviderAssessTask : IProviderAssessTaskService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;

        public ProviderAssessTask(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession)
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
        public async Task<PROVIDER_ASSESS_TASK> GetAsync(string id)
        {
            var query = await _dbContext.Query<PROVIDER_ASSESS_TASK>().Where(c => c.ASSESS_TASK_ID == id).FirstAsync();
            return query;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<PROVIDER_ASSESS_TASK>().Select(c => new
            {
                c.ASSESS_TASK_ID,
                c.AUDITING,
                c.ASSESS_TASK_CODE,
                c.PROVIDER_ID,
                c.PROVIDER_NAME,
                c.FORMULATER_ID,
                c.FORMULATER_NAME,
                c.EXAMINER_ID,
                c.EXAMINER_NAME,
                c.BEGIN_TIME,
                c.END_TIME,
                c.PROVIDER_PRODUCTION,
                c.REMARK,
                c.ADD_USERID,
                c.ADD_DATE,
                c.MODIFY_USERID,
                c.MODIFY_DATE,
                c.TENANT_ID
            }).GetGridData(request);
            return list;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveAsync(SaveRequest<PROVIDER_ASSESS_TASK> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.ASSESS_TASK_ID,
                    c.AUDITING,
                    c.ASSESS_TASK_CODE,
                    c.PROVIDER_ID,
                    c.PROVIDER_NAME,
                    c.FORMULATER_ID,
                    c.FORMULATER_NAME,
                    c.EXAMINER_ID,
                    c.EXAMINER_NAME,
                    c.BEGIN_TIME,
                    c.END_TIME,
                    c.PROVIDER_PRODUCTION,
                    c.REMARK,
                    c.ADD_USERID,
                    c.ADD_DATE,
                    c.MODIFY_USERID,
                    c.MODIFY_DATE,
                    c.TENANT_ID
                },
                c => a => a.ASSESS_TASK_ID == c.ASSESS_TASK_ID
                , BeforeAdd, BeforeUpdate, BeforeDelete, false, null, AfterSave);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(PROVIDER_ASSESS_TASK entity)
        {
            entity.ASSESS_TASK_ID = GuidHelper.NewSnowflakeId().ToString();

            if (string.IsNullOrEmpty(entity.ASSESS_TASK_ID))
            {
                entity.ASSESS_TASK_ID = _userSession.Corp.CorpID;
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(PROVIDER_ASSESS_TASK entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(PROVIDER_ASSESS_TASK entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 保存后验证
        /// </summary>
        private async Task AfterSave(List<PROVIDER_ASSESS_TASK> added, List<PROVIDER_ASSESS_TASK> updated, List<PROVIDER_ASSESS_TASK> deleted)
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
