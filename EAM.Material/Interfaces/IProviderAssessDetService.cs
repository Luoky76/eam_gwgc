using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Interfaces
{
    public interface IProviderAssessDetService : IService
    {
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<GridData> ListAsync(GridRequest request);

        /// <summary>
        /// 根据评估id ASSESS_ID 获取列表
        /// </summary>
        /// <param name="assessId"></param>
        /// <returns></returns>
        public Task<GridData> CertainAssessListAsync(string assessId);

        /// <summary>
        /// 根据ID获取单行记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<PROVIDER_ASSESS_DET> GetAsync(string id);

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<AjaxResult> SaveAsync(SaveRequest<PROVIDER_ASSESS_DET> request);

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public Task<AjaxResult> ComboxData();
    }
}
