using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Special.Interfaces
{
    public interface IDrugCollectRequestService : IService
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
        public Task<DRUG_COLLECT_REQUEST> GetAsync(string id);

        /// <summary>
        /// 根据COLLECT_ID获取列表
        /// </summary>
        /// <param name="collectId"></param>
        /// <returns></returns>
        public Task<GridData> GetCertainCollectIdAsync(string collectId);

        /// <summary>
        /// 获取需要药品SP_ID的需求
        /// </summary>
        /// <param name="spId"></param>
        /// <returns></returns>
        public Task<List<DRUG_COLLECT_REQUEST>> GetCertainSpIdAsync(string spId);

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<AjaxResult> SaveAsync(SaveRequest<DRUG_COLLECT_REQUEST> request);

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public Task<AjaxResult> ComboxData();
    }
}
