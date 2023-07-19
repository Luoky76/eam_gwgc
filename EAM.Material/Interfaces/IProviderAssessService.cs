using EAM.Material.DTO;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Interfaces
{
    public interface IProviderAssessService : IService
    {
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<GridData> ListAsync(GridRequest request);

        /// <summary>
        /// 连接评估任务表后返回列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<GridData> ExtendListAsync(GridRequest request);

        /// <summary>
        /// 根据评估任务ID ASSESS_TASK_ID 返回列表
        /// </summary>
        /// <param name="assessTaskId"></param>
        /// <returns></returns>
        public Task<GridData> GetCertainAssessTaskAsync(string assessTaskId);

        /// <summary>
        /// 连接评估任务表后
        /// 根据评估ID ASSESS_ID 返回列表
        /// </summary>
        /// <param name="assessId"></param>
        /// <returns></returns>
        public Task<PROVIDER_ASSESS_AND_TASK> GetCertainAssessAsync(string assessId);

        /// <summary>
        /// 根据ID获取单行记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<PROVIDER_ASSESS> GetAsync(string id);

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<AjaxResult> SaveAsync(SaveRequest<PROVIDER_ASSESS> request);

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public Task<AjaxResult> ComboxData();
    }
}