using Gksyb.Common;
using Gksyb.Core.Application;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Special.Interfaces
{
    public interface IAssetReportService : IService
    {
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<GridData> ListAsync(GridRequest request);

        /// <summary>
        /// 维修申请列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<GridData> ApplyListAsync(GridRequest request);

        /// <summary>
        /// 维修实施列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<GridData> CheckListAsync(GridRequest request);

        /// <summary>
        /// 委外维修列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<GridData> OutsourceListAsync(GridRequest request);

        /// <summary>
        /// 维修验收列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<GridData> AcceptListAsync(GridRequest request);

        /// <summary>
        /// 根据ID获取单行记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<ASSET_REPORT> GetAsync(string id);

        /// <summary>
        /// 生成主键
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        public string CreatePrimaryKey();

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<AjaxResult> SaveAsync(SaveRequest<ASSET_REPORT> request);

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public Task<AjaxResult> ComboxData();
    }
}
