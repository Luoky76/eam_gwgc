using Gksyb.Core.Common;
using Gksyb.Model.Grid;
using System.Collections;

namespace Gksyb.Core.Interfaces.Common
{
    public interface ICommonService : IService
    {
        /// <summary>
        /// 获取系统时间
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<(DateTime, DateTime)> SysdateAsync(SysdateRequest request);

        /// <summary>
        /// 执行视图获取json数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="view">视图</param>
        /// <param name="parm">变量</param>
        /// <param name="isGridJson"></param>
        /// <returns></returns>
        Task<List<T>> JsonValueAsync<T>(QueryViewRequest request);

        /// <summary>
        /// 执行多个视图获取json数据
        /// </summary>
        /// <param name="parm">变量</param>
        /// <returns></returns>
        Task<IDictionary<string, object>> JsonValueMulAsync(IDictionary<string, IDictionary<string, object>> param);

        /// <summary>
        /// 获取视图配置
        /// </summary>
        /// <param name="view">视图名称</param>
        /// <returns></returns>
        Task<AjaxResult> QueryConfigAsync(string view);

        /// <summary>
        /// 视图查询
        /// </summary>
        /// <returns></returns>
        Task<GridData<IList>> QueryAsync(GridRequest request);

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="view">视图名称</param>
        /// <returns></returns>
        Task RemoveCacheAsync(string view);

        /// <summary>
        /// 清空缓存
        /// </summary>
        /// <returns></returns>
        Task<bool> ClearAsync();

        /// <summary>
        /// 部门
        /// </summary>
        /// <returns></returns>
        public Task<List<string>> GetDeptList(string dept);
    }
}