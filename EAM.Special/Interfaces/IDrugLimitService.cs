using DocumentFormat.OpenXml.Office2010.Excel;
using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Special.Interfaces
{
    public interface IDrugLimitService : IService
    {
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<GridData> ListAsync(GridRequest request);

        /// <summary>
        /// 获取除特定需求单外，剩余药品数量列表
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public Task<GridData> ExtendListAsync(string requestId);

        /// <summary>
        /// 根据ID获取单行记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<DRUG_LIMIT> GetAsync(string id);

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<AjaxResult> SaveAsync(SaveRequest<DRUG_LIMIT> request);

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public Task<AjaxResult> ComboxData();
    }
}
