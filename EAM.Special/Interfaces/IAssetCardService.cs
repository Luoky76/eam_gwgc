using Gksyb.Common;
using Gksyb.Core.Application;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Special.Interfaces
{
    public interface IAssetCardService : IService
    {
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<GridData> ListAsync(GridRequest request);

        /// <summary>
        /// 无形资产列表（软件）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<GridData> SoftwareListAsync(GridRequest request);

        /// <summary>
        /// 固定资产列表（设备）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<GridData> DeviceListAsync(GridRequest request);

        /// <summary>
        /// 根据ID获取单行记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<ASSET_CARD> GetAsync(string id);

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<AjaxResult> SaveAsync(SaveRequest<ASSET_CARD> request);

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public Task<AjaxResult> ComboxData();
    }
}
