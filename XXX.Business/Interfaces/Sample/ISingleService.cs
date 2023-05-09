using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using Gksyb.Model.XXX.Business;

namespace XXX.Business.Interfaces.Sample
{
    public interface ISingleService : IService
    {
        /// <summary>
        /// 获取树形下拉
        /// </summary>
        /// <returns></returns>
        public Task<List<ComboxData>> RoleData();

        /// <summary>
        /// 根据ID获取数据
        /// </summary>
        public Task<SAMPLE_TABLE> GetAsync(string id);

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public Task<GridData> ListAsync(GridRequest request);

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> SaveAsync(SaveRequest<SAMPLE_TABLE> request);
    }
}