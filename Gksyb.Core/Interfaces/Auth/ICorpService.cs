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
        /// corpId的上级公司，如果corpId本身是公司，则取自己
        /// </summary>
        /// <returns></returns>
        Task<CorpInfo> ParentCompany(string corpId);

        /// <summary>
        /// 根据条件获取组织
        /// </summary>
        Task<List<CorpInfo>> FindCorpsAsync(Expression<Func<CorpInfo, bool>> filter = null);
    }
}