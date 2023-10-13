using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Interfaces
{
    public interface ISpOrderStopService : IService
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
        Task<AjaxResult> Save(SaveRequest<SP_ORDER_STOP> request);


        Task<int> Submit(List<string> sids);

        Task<AjaxResult> CancelSubmit(List<string> sids);

        /// <summary>
        /// 获取明细列表信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GridData> DetailListAsync(GridRequest request);

        /// <summary>
        /// 明细保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AjaxResult> DetailSave(SaveRequest<SP_STOP_DET> request);
        /// <summary>
        /// 订单选择列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GridData> SpOrderListAsync(GridRequest request);
    }
}