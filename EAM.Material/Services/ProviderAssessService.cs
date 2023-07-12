using EAM.Material.Interfaces;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Services
{
    public class ProviderAssessService : IProviderAssessService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;

        public ProviderAssessService(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession)
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
        public async Task<PROVIDER_ASSESS> GetAsync(string id)
        {
            var query = await _dbContext.Query<PROVIDER_ASSESS>().Where(c => c.ASSESS_ID == id).FirstAsync();
            return query;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<PROVIDER_ASSESS>().Select(c => new
            {
                c.ASSESS_ID,
                c.AUDITING,
                c.ASSESS_TASK_ID,
                c.EXAMINER_ID,
                c.EXAMINER_NAME,
                c.REMARK,
                c.TOTAL_SCORE,
                c.RESULT,
                c.CREATE_USERID,
                c.CREATEDATE,
                c.MODIFY_USERID,
                c.MODIFYDATE

            }).GetGridData(request);
            return list;
        }

        /// <summary>
        /// 连接评估任务表后返回列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ExtendListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<PROVIDER_ASSESS>()
                .LeftJoin<PROVIDER_ASSESS_TASK>((a, b) => a.ASSESS_TASK_ID == b.ASSESS_TASK_ID)
                .Select((a, b) => new
                {
                    a.ASSESS_ID,
                    a.AUDITING,
                    a.ASSESS_TASK_ID,
                    a.EXAMINER_ID,
                    a.EXAMINER_NAME,
                    a.REMARK,
                    a.TOTAL_SCORE,
                    a.RESULT,
                    a.CREATE_USERID,
                    a.CREATEDATE,
                    a.MODIFY_USERID,
                    a.MODIFYDATE,
                    b.PROVIDER_ID,
                    b.PROVIDER_NAME,
                    b.FORMULATER_ID,
                    b.FORMULATER_NAME,
                    b.BEGIN_TIME,
                    b.END_TIME,
                    b.PROVIDER_PRODUCTION
                }).GetGridData(request);
            return list;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveAsync(SaveRequest<PROVIDER_ASSESS> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.ASSESS_ID,
                    c.AUDITING,
                    c.ASSESS_TASK_ID,
                    c.EXAMINER_ID,
                    c.EXAMINER_NAME,
                    c.REMARK,
                    c.TOTAL_SCORE,
                    c.RESULT,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                },
                c => a => a.ASSESS_TASK_ID == c.ASSESS_TASK_ID
                , BeforeAdd, BeforeUpdate, BeforeDelete, false, null, AfterSave);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(PROVIDER_ASSESS entity)
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
        private async Task BeforeUpdate(PROVIDER_ASSESS entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(PROVIDER_ASSESS entity)
        {
            await Task.CompletedTask;


        }

        /// <summary>
        /// 保存后验证
        /// </summary>
        private async Task AfterSave(List<PROVIDER_ASSESS> added, List<PROVIDER_ASSESS> updated, List<PROVIDER_ASSESS> deleted)
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
