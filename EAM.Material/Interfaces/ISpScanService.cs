using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Interfaces
{
    public interface ISpScanService : IService
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
        Task<AjaxResult> Save(SaveRequest<SP_SCAN> request);

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        /// <returns></returns>
        Task<AjaxResult> ComboxData();

        Task<int> Submit(List<string> sids);
        Task<GridData> DetailListAsync(GridRequest request);
        Task<AjaxResult> DetailSave(SaveRequest<SP_SCAN_DET> request);
        Task<AjaxResult> GenerateDet(string SCAN_ID);
        Task<int> DetSubmit(List<string> sids);
        Task<GridData> DetailAnsListAsync(GridRequest request);
    }
}