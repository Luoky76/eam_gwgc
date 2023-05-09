using Gksyb.Model.UI;
using System.Collections.Concurrent;

namespace Gksyb.Core.Interfaces.Common
{
    public interface IComboxDataService : IService
    {
        /// <summary>
        /// 获取下拉数据<br/> await _comboxDataService.Get(new Dictionary&lt;string, object&gt;(){
        /// <br/>{ "vessel",(Expression&lt;Func&lt;TVESSEL, bool&gt;&gt;)(c => c.FLAG == "1")}
        /// <br/>});
        /// </summary>
        /// <param name="views"></param>
        /// <returns></returns>
        public Task<ConcurrentDictionary<string, List<ComboxData>>> Get(IDictionary<string, object> views);
    }
}