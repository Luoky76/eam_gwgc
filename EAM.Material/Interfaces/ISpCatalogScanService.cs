using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System.Collections.Concurrent;

namespace EAM.Material.Interfaces
{
    public interface ISpCatalogScanService : IService
    {
        /// <summary>
        /// 获取下拉
        /// </summary>
        /// <returns></returns>
        public Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData();

        /// <summary>
        /// 获取列表信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GridData> ListAsync(GridRequest request);

        /// <summary>
        /// 导入功能
        /// </summary>
        /// <returns></returns>
        public Task<GridData> ImportSpList(GridRequest request);

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <param name="requestdet"></param>
        /// <returns></returns>
        Task<AjaxResult> SaveAsync(SaveRequest<SP_CATALOG_SCAN> request, SaveRequest<SP_CATALOG_SCAN_DET> requestdet);

        Task<GridData> DetailListAsync(GridRequest request);
    }
}