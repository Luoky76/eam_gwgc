using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Interfaces
{
    public interface ISpPurplanService : IService
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
        Task<AjaxResult> Save(SaveRequest<SP_PURPLAN> request);

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        /// <returns></returns>
        Task<AjaxResult> ComboxData();

        Task<int> Submit(List<string> sids);
        Task<AjaxResult> CancelSubmit(List<string> sids);
        /// <summary>
        /// 明细-列表
        /// </summary>
        /// <param name="PURPLAN_ID"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GridData> DetailListAsync(string PURPLAN_ID, GridRequest request);
        /// <summary>
        /// 明细保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AjaxResult> DetailSave(SaveRequest<SP_PURPLAN_DET> request);
    }
}