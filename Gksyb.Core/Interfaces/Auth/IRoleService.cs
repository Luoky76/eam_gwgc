using Gksyb.Model.UI;

namespace Gksyb.Core.Interfaces.Auth
{
    public interface IRoleService : IService
    {
        /// <summary>
        /// 下拉数据
        /// </summary>
        /// <returns></returns>
        Task<List<ComboxData>> ComboxDataAsync();

        /// <summary>
        /// 获取所有角色
        /// </summary>
        Task<List<RoleInfo>> AllRoles();

        /// <summary>
        /// 根据条件返回角色
        /// </summary>
        Task<List<RoleInfo>> FindRoles(string CorpId);
    }
}