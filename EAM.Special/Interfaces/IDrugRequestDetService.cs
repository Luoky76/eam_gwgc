using DocumentFormat.OpenXml.Office2010.Excel;
using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Special.Interfaces
{
    public interface IDrugRequestDetService : IService
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
        public Task<DRUG_REQUEST_DET> GetAsync(string id);

        /// <summary>
        /// 根据药品需求ID REQUEST_ID 获取多行记录
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public Task<GridData> GetCertainRequestAsync(string requestId);

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<AjaxResult> SaveAsync(SaveRequest<DRUG_REQUEST_DET> request);

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public Task<AjaxResult> ComboxData();
    }
}
