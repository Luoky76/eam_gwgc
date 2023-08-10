using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Interfaces
{
    public interface ISpOrderService : IService
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
        Task<AjaxResult> Save(SaveRequest<SP_ORDER> request);

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        /// <returns></returns>
        Task<AjaxResult> ComboxData();


        Task<int> Submit(List<string> sids);

        /// <summary>
        /// 获取明细列表信息
        /// </summary>
        /// <param name="ORDER_ID"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GridData> DetailListAsync(string ORDER_ID, GridRequest request);

        /// <summary>
        /// 明细保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AjaxResult> DetailSave(SaveRequest<SP_ORDER_DETAIL> request);

        Task<GridData> OrderOverListAsync(GridRequest request);
        /// <summary>
        /// 订单完成情况
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GridData> OrderListAsync(GridRequest request);
    }
}