using EAM.Material.Interfaces;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

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
        /// 生成主键
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        public string CreatePrimaryKey()
        {
            return GuidHelper.NewSnowflakeId().ToString();
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
                c.BEGIN_TIME,
                c.END_TIME,
                c.PROVIDER_PRODUCTION,
                c.REMARK,
                c.CREATE_USERID,
                c.CREATEDATE,
                c.MODIFY_USERID,
                c.MODIFYDATE
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
                    c.BEGIN_TIME,
                    c.END_TIME,
                    c.PROVIDER_PRODUCTION,
                    c.REMARK,
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
        private async Task BeforeAdd(PROVIDER_ASSESS_TASK entity)
        {
            //获取并设置主键
            if (entity.ASSESS_TASK_ID.IsNullOrEmpty())
            {
                entity.ASSESS_TASK_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            //设置记录状态为已提交
            entity.AUDITING = "1";
            //设置任务制定人为当前登录者
            //entity.FORMULATER_NAME = window.session.RealName;
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
                    {"Auditing", null },
                    {"ProviderName", null },
                    {"User", null }
                });

                return AjaxResult.Success(data);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }

        /// <summary>
        /// 连接评估表PROVIDER_ASSESS
        /// 返回评估结果
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<GridData> ResultListAsync(GridRequest request)
        {
            // 评估任务id 供应商id 供应商名 供应商产品 总分 平均分 最高分 最低分 实际评分人数 计划评分人数 任务制定人id 任务制定人

            var pat = _dbContext.Query<PROVIDER_ASSESS_TASK>();
            var pa = _dbContext.Query<PROVIDER_ASSESS>();
            // 记录状态为已提交的有效实际评分
            var g1 = pa.Where(a => a.AUDITING == "1")
                .GroupBy(a => a.ASSESS_TASK_ID)
                .Select(a => new
                {
                    a.ASSESS_TASK_ID,
                    TOTAL_SCORE_SUM = Sql.Sum(a.TOTAL_SCORE),
                    AVERAGE_SCORE = Sql.Average(a.TOTAL_SCORE),
                    MAX_SCORE = Sql.Max(a.TOTAL_SCORE),
                    MIN_SCORE = Sql.Min(a.TOTAL_SCORE),
                    EXAMINER_CNT_ACTUAL = Sql.Count()
                });
            // 计划评分人数
            var g2 = pa.GroupBy(a => a.ASSESS_TASK_ID)
                .Select(a => new
                {
                    a.ASSESS_TASK_ID,
                    EXMAINER_CNT = Sql.Count()
                });
            // 连接三表
            var list = await pat.InnerJoin(g1, (a, b) => a.ASSESS_TASK_ID == b.ASSESS_TASK_ID)
                .InnerJoin(g2, (a, b, c) => a.ASSESS_TASK_ID == c.ASSESS_TASK_ID)
                .Select((a, b, c) => new
                {
                    a.ASSESS_TASK_ID,
                    a.PROVIDER_ID,
                    a.PROVIDER_NAME,
                    a.PROVIDER_PRODUCTION,
                    a.FORMULATER_ID,
                    a.FORMULATER_NAME,
                    b.TOTAL_SCORE_SUM,
                    b.AVERAGE_SCORE,
                    b.MAX_SCORE,
                    b.MIN_SCORE,
                    b.EXAMINER_CNT_ACTUAL,
                    c.EXMAINER_CNT
                })
                .GetGridData(request);

            return list;
        }
    }
}
