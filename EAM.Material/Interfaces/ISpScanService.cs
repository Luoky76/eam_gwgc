namespace EAM.Material.Interfaces
{
    public interface ISpScanService : IService
    {
        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        /// <returns></returns>
        Task<AjaxResult> ComboxData();
    }
}