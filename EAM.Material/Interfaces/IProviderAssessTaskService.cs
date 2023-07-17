using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Interfaces
{
    public interface IProviderAssessTaskService : IService
    {
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<GridData> ListAsync(GridRequest request);

        /// <summary>
        /// 根据ID获取单行记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<PROVIDER_ASSESS_TASK> GetAsync(string id);

        /// <summary>
        /// 生成主键
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        public string CreatePrimaryKey();

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<AjaxResult> SaveAsync(SaveRequest<PROVIDER_ASSESS_TASK> request);

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public Task<AjaxResult> ComboxData();

        /// <summary>
        /// 连接评估表PROVIDER_ASSESS
        /// 返回评估结果
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<GridData> ResultListAsync(GridRequest request);
    }
}
