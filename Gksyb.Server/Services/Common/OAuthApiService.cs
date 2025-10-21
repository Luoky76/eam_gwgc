using Flurl;
using Flurl.Http;
using Flurl.Http.Content;
using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Auth.Dtos;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Core;
using Microsoft.Extensions.Logging;

namespace Gksyb.Server.Services.Common
{
    public class OAuthApiService : IOAuthApiService
    {
        private const int Unauthorized = 999;
        private readonly IDbContext _dbContext;
        private readonly ILogger<OAuthApiService> _logger;
        private readonly ScopeUser _user;

        public OAuthApiService(IDbContext dbContext, ScopeUser user, ILogger<OAuthApiService> logger)
        {
            _dbContext = dbContext;
            _user = user;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<T> PostJsonAsync<T>(string appId, string segment, object data)
        {
            string json = data is string sdata ? sdata : data.ToJson();
            return await PostAsync<T>(appId, segment, () => new CapturedJsonContent(json));
        }

        /// <inheritdoc/>
        public async Task<T> PostAsync<T>(string appId, string segment, Func<HttpContent> func)
        {
            return await ApiInvoke<T>(appId, HttpMethod.Post, request =>
            {
                request.AppendPathSegment(segment);
                return func();
            });
        }

        /// <inheritdoc/>
        public async Task<T> ApiInvoke<T>(string appId, HttpMethod verb, Func<FlurlRequest, HttpContent> func)
        {
            HttpContent content = null;
            string response = null;
            var request = await GetRequest(appId);
            DateTime startTime = DateTime.Now;
            try
            {
                content = func(request);
                var flurlResponse = await request.SendAsync(verb, content);
                if (flurlResponse.StatusCode == Unauthorized)
                {
                    await RefreshAccessTokenAsync(appId);
                    request = await GetRequest(appId);
                    content = func(request);
                    flurlResponse = await request.SendAsync(verb, content);
                }
                response = await flurlResponse.GetStringAsync();
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)response;
                }
                var model = response.ToObject<T>();
                return model;
            }
            catch (FlurlHttpException ex)
            {
                response = await ex.GetResponseStringAsync();
                throw new MessageException($"{response}");
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                throw;
            }
            finally
            {
                var elapsed = DateTime.Now - startTime;
                var json = content is CapturedStringContent stringContent ? stringContent.Content : content?.Headers?.ContentType?.ToString();
                _logger.LogInformation(new LogPath(appId), $"请求:{request.Url},耗时:{elapsed.TotalMilliseconds},结果:{response},参数:{json}");
            }
        }

        /// <summary>
        /// 获取请求对象
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        private async Task<FlurlRequest> GetRequest(string appId)
        {
            var appInfo = await GetAppInfoAsync(appId);
            var token = await GetAccessToken(appId);
            var request = new FlurlRequest(appInfo.Url).AllowHttpStatus(Unauthorized.ToString());
            request.WithHeader(appInfo.TokenKey, token);
            if (_user != null)
            {
                var apiInfo = ApiInfoValue(appInfo.Secret);
                request.WithHeader(appInfo.ApiInfoKey, apiInfo);
            }
            return request;
        }

        /// <summary>
        /// 获取access_token
        /// </summary>
        private async Task<string> GetAccessToken(string appId)
        {
            if (Tokens.TryGetValue(appId, out var token) && token != null && !token.IsExpired)
            {
                return token.AccessToken;
            }
            return await RefreshAccessTokenAsync(appId);
        }

        /// <summary>
        /// 刷新access_token
        /// </summary>
        private async Task<string> RefreshAccessTokenAsync(string appId)
        {
            var appInfo = await RefreshAppInfoAsync(appId);
            var now = await GetAppNowAsync(appInfo.Url);
            var request = new OAuthRequest<string>()
            {
                AppId = appId,
                Body = Guid.NewGuid().ToString("N"),
                TimeStamp = now
            };
            request.Sign = request.CalcuSign(appInfo.Secret);
            var result = await appInfo.Url.AppendPathSegment("oauth/accessToken").PostJsonAsync(request).ReceiveJson<AjaxResult<AccessTokenResponse>>();
            MessageException.ThrowIf(result.IsError, result.Message);
            Tokens[appId] = result.Data;
            return result.Data.AccessToken;
        }

        /// <summary>
        /// 获取应用当前时间
        /// </summary>
        private static async Task<DateTime> GetAppNowAsync(string url)
        {
            var result = await url.AppendPathSegment("oauth/now").GetJsonAsync<AjaxResult<DateTime>>();
            MessageException.ThrowIf(result.IsError, result.Message);
            return result.Data;
        }

        /// <summary>
        /// 获取应用信息
        /// </summary>
        private async Task<AppInfo> GetAppInfoAsync(string appId)
        {
            if (Apps.TryGetValue(appId, out var appInfo))
            {
                return appInfo;
            }
            return await RefreshAppInfoAsync(appId);
        }

        /// <summary>
        /// 刷新应用信息
        /// </summary>
        private async Task<AppInfo> RefreshAppInfoAsync(string appId)
        {
            var code = await _dbContext.Query<BC_CODE>().Where(c => c.CODE_TYPE == "OAuthApi" && c.CODE_EN == appId).FirstOrDefaultAsync();
            MessageException.ThrowIf(code == null, $"未找到应用编码:{appId}的OAuthApi配置");
            var array = $"{(code.REMARK ?? "")}@#{HttpContextExtensions.TokenName}@#{IApiUserInfoService.HeaderKey}".Split("@#");
            var appInfo = new AppInfo
            {
                Secret = code.CODE_CN,
                Url = array[0].TrimEnd('/'),
                TokenKey = array[1],
                ApiInfoKey = array[2]
            };
            Apps[appId] = appInfo;
            return appInfo;
        }

        /// <summary>
        /// 用户附加信息
        /// </summary>
        private string ApiInfoValue(string secret)
        {
            var userInfo = new ApiUserInfo
            {
                UserID = _user.UserID,
                UserName = _user.UserName,
                RealName = _user.RealName,
                Class = _user.Class,
                WorkerCode = _user.WorkerCode
            };
            return CryptographyHelper.EncryptSM4(userInfo.ToMiniJson(), secret);
        }

        /// <summary>
        /// 缓存的access_token
        /// </summary>
        private readonly static Dictionary<string, AccessTokenResponse> Tokens = new();

        /// <summary>
        /// 缓存的应用信息
        /// </summary>
        private readonly static Dictionary<string, AppInfo> Apps = new();

        class AccessTokenResponse
        {
            /// <summary>
            /// 接口调用凭证
            /// </summary>
            public string AccessToken { get; set; }


            private long _expiresIn;
            private DateTime _expiresTime;

            /// <summary>
            /// 接口调用凭证超时时间，单位（秒）
            /// </summary>
            public long ExpiresIn
            {
                get
                {
                    return _expiresIn;
                }
                set
                {
                    _expiresTime = DateTime.Now.AddSeconds(value);
                    _expiresIn = value;
                }
            }

            /// <summary>
            /// 过期时间
            /// </summary>
            public bool IsExpired => DateTime.Now >= _expiresTime;
        }

        class AppInfo
        {

            /// <summary>
            /// 密钥
            /// </summary>
            public string Secret { get; set; }

            /// <summary>
            /// 地址
            /// </summary>
            public string Url { get; set; }

            /// <summary>
            /// token header头
            /// </summary>
            public string TokenKey { get; set; }

            /// <summary>
            /// Api header头
            /// </summary>
            public string ApiInfoKey { get; set; }
        }
    }
}