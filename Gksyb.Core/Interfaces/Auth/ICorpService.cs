using Gksyb.Model.UI;
using System.Linq.Expressions;

namespace Gksyb.Core.Interfaces.Auth
{
    public interface ICorpService : IService
    {
        /// <summary>
        /// 下拉数据
        /// </summary>
        /// <returns></returns>
        Task<List<ComboxData>> ComboxDataAsync(bool isAll = false);

        /// <summary>
        /// 用户可操作的公司
        /// </summary>
        /// <returns></returns>
        Task<List<CorpInfo>> Corps();

        /// <summary>
        /// 根据条件获取组织
        /// </summary>
        Task<List<CorpInfo>> FindCorpsAsync(Expression<Func<CorpInfo, bool>> filter = null);
    }
}