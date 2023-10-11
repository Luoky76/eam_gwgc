using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Interfaces
{
    public interface ISpCollectService : IService
    {
        /// <summary>
        /// 获取列表信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GridData> ListAsync(GridRequest request);

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AjaxResult> Save(SaveRequest<SP_COLLECT> request);

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        /// <returns></returns>
        Task<AjaxResult> ComboxData();

        Task<int> Submit(List<string> sids);
        Task<GridData> DetailListAsync(GridRequest request);
        Task<AjaxResult> DetailSave(SaveRequest<SP_COLLECT_DET> request);
        Task<GridData> RequestListAsync(GridRequest request);
        Task<AjaxResult> RequestSave(SaveRequest<SP_COLLECT_REQUEST> request);
        Task<GridData> SpApplyListAsync(GridRequest request);

        Task<int> SelectApply(List<string> SpdetID, string Cid);
    }
}