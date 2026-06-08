using Gksyb.Model;
using Gksyb.Model.Grid;

namespace Gksyb.Core.Interfaces.Material
{
    public interface ISpStoreService : IService
    {
        /// <summary>
        /// 保存库存
        /// </summary>
        Task<AjaxResult> SaveAsync(SaveRequest<SP_STORE> request);

        /// <summary>
        /// 保存库存流水
        /// </summary>
        Task<AjaxResult> DetSaveAsync(SaveRequest<SP_STORE_WATER> request);
    }
}
