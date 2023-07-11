using EAM.Material.Interfaces;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Services
{
    public class ProviderAssessTaskDet :  IProviderAssessTaskDetService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;

        public ProviderAssessTaskDet(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userSession = userSession;
        }

        /// <summary>
        /// 根据主键ID获取单行记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<PROVIDER_ASSESS_TASK_DET> GetAsync(string id)
        {
            var query = await _dbContext.Query<PROVIDER_ASSESS_TASK_DET>().Where(c => c.ASSESS_TASK_DET_ID == id).FirstAsync();
            return query;
        }

        /// <summary>
        /// 根据评估任务ID ASSESS_TASK_ID 获取多行记录
        /// </summary>
        /// <param name="assessTaskId"></param>
        /// <returns></returns>
        public async Task<GridData> GetAssessTaskAsync(string assessTaskId)
        {
            var query = await _dbContext.Query<PROVIDER_ASSESS_TASK>()
                .InnerJoin<PROVIDER_ASSESS_TASK_DET>((a, b) => a.ASSESS_TASK_ID == b.ASSESS_TASK_ID)
                .LeftJoin<PROVIDER_ASSESS_BASE>((a, b, c) => b.ASSESS_BASE_ID == c.ASSESS_BASE_ID)
                .Where((a, b, c) => a.ASSESS_TASK_ID == assessTaskId)
                .Select((a, b, c) => new
                {
                    a.ASSESS_TASK_ID,
                    b.ASSESS_TASK_DET_ID,
                    c.ASSESS_BASE_ID,
                    c.CONTENT,
                    c.IS_VALID
                }).GetGridData(null);
            return query;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<PROVIDER_ASSESS_TASK_DET>().Select(c => new
            {
                c.ASSESS_TASK_DET_ID,
                c.ASSESS_TASK_ID,
                c.ASSESS_BASE_ID,
                c.ADD_USERID,
                c.ADD_DATE,
                c.MODIFY_USERID,
                c.MODIFY_DATE
            }).GetGridData(request);
            return list;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveAsync(SaveRequest<PROVIDER_ASSESS_TASK_DET> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.ASSESS_TASK_DET_ID,
                    c.ASSESS_TASK_ID,
                    c.ASSESS_BASE_ID,
                    c.ADD_USERID,
                    c.ADD_DATE,
                    c.MODIFY_USERID,
                    c.MODIFY_DATE
                },
                c => a => a.ASSESS_TASK_DET_ID == c.ASSESS_TASK_DET_ID
                , BeforeAdd, BeforeUpdate, BeforeDelete, false, null, AfterSave);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(PROVIDER_ASSESS_TASK_DET entity)
        {
            entity.ASSESS_TASK_DET_ID = GuidHelper.NewSnowflakeId().ToString();

            if (string.IsNullOrEmpty(entity.ASSESS_TASK_DET_ID))
            {
                entity.ASSESS_TASK_DET_ID = _userSession.Corp.CorpID;
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(PROVIDER_ASSESS_TASK_DET entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(PROVIDER_ASSESS_TASK_DET entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 保存后验证
        /// </summary>
        private async Task AfterSave(List<PROVIDER_ASSESS_TASK_DET> added, List<PROVIDER_ASSESS_TASK_DET> updated, List<PROVIDER_ASSESS_TASK_DET> deleted)
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
                    {"AssessBaseContent", null}
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