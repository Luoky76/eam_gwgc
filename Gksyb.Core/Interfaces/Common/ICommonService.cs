using Gksyb.Core.Common;
using Gksyb.Model.Grid;

namespace Gksyb.Core.Interfaces.Common
{
    public interface ICommonService : IService
    {
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
        Task<IDictionary<string, object>> JsonValueMulAsync(IDictionary<string, IDictionary<string, object>> param);

        /// <summary>
        /// 视图查询
        /// </summary>
        Task<GridData<List<T>>> QueryAsync<T>(GridRequest request);
    }
}