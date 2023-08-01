using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Interfaces
{
    public interface ISpareApplyService : IService
    {
        #region 物资编码申请
        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        /// <returns></returns>
        Task<AjaxResult> ComboxData();

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
        Task<AjaxResult> Save(SaveRequest<SPARE_APPLY> request);
        /// <summary>
        /// 申请
        /// </summary>
        /// <param name="memo"></param>
        /// <returns></returns>
        Task<string> ApplySave(string memo);
        Task<int> Submit(List<string> sids);

        /// <summary>
        /// 明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GridData> DetailListAsync(GridRequest request);
        Task<AjaxResult> DetailSave(SaveRequest<SPARE_APPLY_DET> request);
        #endregion

        #region 物资编码禁用
        Task<GridData> SpcatalogListAsync(GridRequest request);
        Task<GridData> SpDisableListAsync(GridRequest request);
        /// <summary>
        /// 禁用申请保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AjaxResult> SpDisableSave(SaveRequest<SP_DISABLE> request);
        /// <summary>
        /// 禁用申请提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        Task<int> SpDisableSubmit(List<string> sids);
        /// <summary>
        /// 禁用明细列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GridData> SpDisableDetailListAsync(GridRequest request);
        /// <summary>
        /// 明细保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AjaxResult> SpDisableDetailSave(SaveRequest<SP_DISABLE_DET> request);
        #endregion

        #region 物资编码启用
        Task<GridData> SpEnableListAsync(GridRequest request);
        /// <summary>
        /// 启用申请
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AjaxResult> SpEnableSave(SaveRequest<SP_ENABLE> request);
        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        Task<int> SpEnableSubmit(List<string> sids);
        /// <summary>
        /// 明细列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GridData> SpEnableDetailListAsync(GridRequest request);
        /// <summary>
        /// 明细保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AjaxResult> SpEnableDetailSave(SaveRequest<SP_ENABLE_DET> request);
        #endregion
    }
}