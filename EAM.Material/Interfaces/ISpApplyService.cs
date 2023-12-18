using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Http;

namespace EAM.Material.Interfaces
{
    public interface ISpApplyService : IService
    {
        /// <summary>
        /// 获取列表信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GridData> ListAsync(GridRequest request);

        Task<SP_APPLY> GetApplyDetail(string ID);

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <param name="requestdet"></param>
        /// <returns></returns>
        Task<AjaxResult> Save(SaveRequest<SP_APPLY> request, SaveRequest<SP_APPLY_DETAIL> requestdet);

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        /// <returns></returns>
        Task<AjaxResult> ComboxData();

        Task<int> Submit(List<string> sids);
        Task<AjaxResult> CancelSubmit(List<string> sids);
        Task<GridData> DetailListAsync(GridRequest request);
        Task<AjaxResult> DetailSave(SaveRequest<SP_APPLY_DETAIL> request);

        Task<GridData> ApplyListAsync(GridRequest request);
        Task<AjaxResult> ApplyComboxData();
        Task<AjaxResult> ApplyDetFlowAsync(string SPDET_ID);

        Task<int> CheckSubmit(List<string> sids);
        Task<AjaxResult> CheckCancelSubmit(List<string> sids);
        Task<AjaxResult> ImportInDetail([FileOptions("xlsx,xls")] IFormFile formFile, string folder, string sid);
    }
}