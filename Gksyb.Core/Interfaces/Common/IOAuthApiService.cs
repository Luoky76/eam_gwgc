using Flurl.Http;

namespace Gksyb.Core.Interfaces.Common
{
    public interface IOAuthApiService : IService
    {
        /// <summary>
        /// post json请求
        /// </summary>
        /// <param name="appId">应用编码</param>
        /// <param name="segment">路径</param>
        /// <param name="data">数据</param>
        Task<T> PostJsonAsync<T>(string appId, string segment, object data);

        /// <summary>
        /// post 请求
        /// </summary>
        /// <param name="appId">应用编码</param>
        /// <param name="segment">路径</param>
        /// <param name="func">请求体委托</param>
        Task<T> PostAsync<T>(string appId, string segment, Func<HttpContent> func);

        /// <summary>
        /// api调用(自动带上access_token和用户信息)
        /// </summary>
        /// <typeparam name="T">返回对象</typeparam>
        /// <param name="appId">应用编码</param>
        /// <param name="method">请求类型</param>
        /// <param name="func">请求逻辑</param>
        Task<T> ApiInvoke<T>(string appId, HttpMethod method, Func<FlurlRequest, HttpContent> func);
    }
}