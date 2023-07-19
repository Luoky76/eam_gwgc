using EAM.Material.DTO;
using EAM.Material.Interfaces;
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
                    a.REMARK,
                    a.TOTAL_SCORE,
                    a.RESULT,
                    b.PROVIDER_ID,
                    b.PROVIDER_NAME,
                    b.BEGIN_TIME,
                    b.END_TIME,
                    b.PROVIDER_PRODUCTION,
                    b.CREATE_USERID,
                }).GetGridData(request);
            return list;
        }

        /// <summary>
        /// 根据评估任务ID ASSESS_TASK_ID 返回列表
        /// </summary>
        /// <param name="assessTaskId"></param>
        /// <returns></returns>
        public async Task<GridData> GetCertainAssessTaskAsync(string assessTaskId)
        {
            var list = await _dbContext.Query<PROVIDER_ASSESS>()
                .Select(c => new
                {
                    c.ASSESS_ID,
                    c.AUDITING,
                    c.ASSESS_TASK_ID,
                    c.EXAMINER_ID,
                    c.REMARK,
                    c.TOTAL_SCORE,
                    c.RESULT,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                })
                .Where(c => c.ASSESS_TASK_ID == assessTaskId )
                .GetGridData(null);
            return list;
        }

        /// <summary>
        /// 连接评估任务表后
        /// 根据评估ID ASSESS_ID 返回单行记录
        /// </summary>
        /// <param name="assessId"></param>
        /// <returns></returns>
        public async Task<PROVIDER_ASSESS_AND_TASK> GetCertainAssessAsync(string assessId)
        {
            var row = await _dbContext.Query<PROVIDER_ASSESS>()
                .LeftJoin<PROVIDER_ASSESS_TASK>((a, b) => a.ASSESS_TASK_ID == b.ASSESS_TASK_ID)
                .Select((a, b) => new PROVIDER_ASSESS_AND_TASK
                {
                    ASSESS_ID = a.ASSESS_ID,
                    AUDITING = a.AUDITING,
                    ASSESS_TASK_ID = a.ASSESS_TASK_ID,
                    EXAMINER_ID = a.EXAMINER_ID,
                    REMARK = a.REMARK,
                    TOTAL_SCORE = a.TOTAL_SCORE,
                    RESULT = a.RESULT,
                    PROVIDER_ID = b.PROVIDER_ID,
                    PROVIDER_NAME = b.PROVIDER_NAME,
                    FORMULATER_ID = a.CREATE_USERID,
                    BEGIN_TIME = b.BEGIN_TIME,
                    END_TIME = b.END_TIME,
                    PROVIDER_PRODUCTION = b.PROVIDER_PRODUCTION,
                })
                .Where(c => c.ASSESS_ID == assessId)
                .FirstAsync();
            return row;
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
                    c.REMARK,
                    c.TOTAL_SCORE,
                    c.RESULT,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                },
                c => a => a.ASSESS_ID == c.ASSESS_ID
                , BeforeAdd, null, null, false, null, null);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(PROVIDER_ASSESS entity)
        {
            if (entity.ASSESS_ID.IsNullOrEmpty())
            {
                entity.ASSESS_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            //AUDITING 默认为未提交
            if (entity.AUDITING.IsNullOrEmpty())
            {
                entity.AUDITING = "0";
            }
            var list = await _dbContext.Query<PROVIDER_ASSESS>()
                .Select(c => new
                {
                    c.ASSESS_TASK_ID,
                    c.EXAMINER_ID
                })
                .Where(c => c.ASSESS_TASK_ID == entity.ASSESS_TASK_ID && c.EXAMINER_ID == entity.EXAMINER_ID)
                .ToListAsync();
            if (list.Any())
            {
                throw new MessageException("请勿重复添加评分人！");
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
                    {"Auditing", null },
                    {"User", null },
                    {"AssessBaseContent", null }
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
