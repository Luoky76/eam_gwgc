using Gksyb.Model.UI;

namespace Gksyb.Core.Interfaces.Common
{
    public interface IBCCodeService : IService
    {
        /// <summary>
        /// 字典下拉数据
        /// </summary>
        /// <returns></returns>
        Task<List<ComboxData>> Get(string codeType, string code = null);
    }
}