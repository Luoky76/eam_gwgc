using Gksyb.Model.UI;

namespace Gksyb.Core.Interfaces.Auth
{
    public interface IUserService : IService
    {
        /// <summary>
        /// 下拉数据
        /// </summary>
        /// <returns></returns>
        Task<List<ComboxData>> ComboxDataAsync(string corp = null);

        /// <summary>
        /// 用户组
        /// </summary>
        /// <returns></returns>
        Task<List<ComboxData>> GroupsAsync();

        /// <summary>
        /// 获取所有用户
        /// </summary>
        Task<List<UserInfo>> AllUsers();

        /// <summary>
        /// 获取无公司、相同公司、子公司的用户
        /// </summary>
        /// <param name="filterSelf">过滤当前用户</param>
        /// <returns></returns>
        Task<List<UserInfo>> Users(bool filterSelf = true);

        /// <summary>
        /// 根据用户ID返回用户信息
        /// </summary>
        /// <param name="ids">用户id</param>
        /// <param name="skipCorp">返回数据跳过获取用户公司信息</param>
        /// <returns></returns>
        Task<List<UserInfo>> Find(List<long?> ids, bool skipCorp = true);

        /// <summary>
        /// 根据给定信息查找操作人
        /// </summary>
        public Task<List<UserInfo>> FindOperators(FindOperatorInfo info);

        /// <summary>
        /// 根据公司返回用户
        /// </summary>
        /// <param name="CorpId">公司</param>
        /// <param name="station">岗位</param>
        /// <param name="skipCorp">返回数据跳过获取用户公司信息</param>
        /// <returns></returns>
        Task<List<UserInfo>> FindByCorp(string CorpId, string station = null, bool skipCorp = false);

        /// <summary>
        /// 查找指定岗位的人员
        /// </summary>
        /// <param name="CorpId">公司</param>
        /// <param name="station">岗位</param>
        /// <param name="skipCorp">返回数据跳过获取用户公司信息</param>
        /// <returns></returns>
        Task<List<UserInfo>> FindByStation(string CorpId, string station, bool skipCorp = true);

        /// <summary>
        /// 查找指定角色的用户
        /// </summary>
        /// <param name="roles">角色</param>
        /// <param name="skipCorp">返回数据跳过获取用户公司信息</param>
        /// <returns></returns>
        Task<List<UserInfo>> FindByRole(List<long?> roles, bool skipCorp = true);
    }
}