using EAM.Material.Interfaces;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
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
        private readonly IUserService _userService;

        public ProviderAssessTask(IDbContext dbContext, IComboxDataService comboxDataService, IUserService userService)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userService = userService;
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
                , BeforeAdd, null, null, false, null, AfterSave, false);
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
            //设置任务编码
            if (entity.ASSESS_TASK_CODE.IsNullOrEmpty())
            {

                var sysdate = await _dbContext.GetSysdate();
                //var dateCode = sysdate.ToString("yyyyMMddHH");
                var dateCode = sysdate.ToString();

                var list = await _dbContext.Query<PROVIDER_ASSESS_TASK>()
                    .Select(a => new
                    {
                        MAX_ASSESS_TASK_CODE = Sql.Max(a.ASSESS_TASK_CODE)
                    })
                    .ToListAsync();
                if (list.Any())
                {
                    string code = list[0].MAX_ASSESS_TASK_CODE;
                    string last = code.Remove(3, code.Length - 3);

                }
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
            //级联删除评估任务明细
            foreach (var entity in deleted)
            {
                var list = await _dbContext.Query<PROVIDER_ASSESS>()
                    .Where(c => c.ASSESS_TASK_ID == entity.ASSESS_TASK_ID)
                    .Select(c => new
                    {
                        c.ASSESS_ID
                    })
                    .ToListAsync();
                await _dbContext.DeleteAsync<PROVIDER_ASSESS_TASK_DET>(c => c.ASSESS_TASK_ID == entity.ASSESS_TASK_ID);
                await _dbContext.DeleteAsync<PROVIDER_ASSESS>(c => c.ASSESS_TASK_ID == entity.ASSESS_TASK_ID);
                
                foreach (var assessEntity in list)
                {
                    await _dbContext.DeleteAsync<PROVIDER_ASSESS_DET>(c => c.ASSESS_ID == assessEntity.ASSESS_ID);
                }
            }
            //级联删除
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
                    {"ProviderName", null }
                });
                data.TryAdd("User", await _userService.ComboxDataAsync());
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
            // 评估任务id 供应商id 供应商名 供应商产品 总分 平均分 最高分 最低分 实际评分人数 计划评分人数 任务制定人id

            var query1 = _dbContext.Query<PROVIDER_ASSESS_TASK>()
                .LeftJoin<PROVIDER_ASSESS>((a, b) => a.ASSESS_TASK_ID == b.ASSESS_TASK_ID)
                .Select((a, b) => new
                {
                    a.ASSESS_TASK_ID,
                    a.PROVIDER_ID,
                    a.PROVIDER_NAME,
                    a.PROVIDER_PRODUCTION,
                    TOTAL_SCORE_SUM = Sql.Sum(b.TOTAL_SCORE),
                    AVERAGE_SCORE = Sql.Average(b.TOTAL_SCORE),
                    MAX_SCORE = Sql.Max(b.TOTAL_SCORE),
                    MIN_SCORE = Sql.Min(b.TOTAL_SCORE),
                    EXAMINER_CNT_ACTUAL = Sql.Count(b.AUDITING == "1"),
                    EXMAINER_CNT = Sql.Count()
                })
                .GroupBy(c => c.ASSESS_TASK_ID)
                .AndBy(c => c.PROVIDER_ID)
                .AndBy(c => c.PROVIDER_NAME)
                .AndBy(c => c.PROVIDER_PRODUCTION)
                .Select(c => new {
                    c.ASSESS_TASK_ID,
                    c.PROVIDER_ID,
                    c.PROVIDER_NAME,
                    c.PROVIDER_PRODUCTION,
                    c.TOTAL_SCORE_SUM,
                    c.AVERAGE_SCORE,
                    c.MAX_SCORE,
                    c.MIN_SCORE,
                    c.EXAMINER_CNT_ACTUAL,
                    c.EXMAINER_CNT
                });

            /*var query2 = _dbContext.Query<PROVIDER_ASSESS_TASK>()
                .LeftJoin<PROVIDER_ASSESS>((a, b) => a.ASSESS_TASK_ID == b.ASSESS_TASK_ID)
                .Select((a, b) => new
                {
                    a.ASSESS_TASK_ID,
                    b.AUDITING,
                    EXAMINER_CNT_ACTUAL = Sql.Count()
                })
                .GroupBy(c => c.ASSESS_TASK_ID)
                .Having(c=> c.AUDITING == "1")
                .Select(c => new
                {
                    c.ASSESS_TASK_ID,
                    c.EXAMINER_CNT_ACTUAL
                });*/

            

            var list = await query1.GetGridData(request);

            return list;
        }
    }
}
