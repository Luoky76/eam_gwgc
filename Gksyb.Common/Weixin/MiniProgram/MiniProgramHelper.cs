using Flurl.Http;
using Gksyb.Common.Static;
using Microsoft.Extensions.DependencyInjection;

namespace Gksyb.Common.Weixin
{
    public static class MiniProgramHelper
    {
        internal static readonly string ApiHost = WeixinSetting.ApiHost;

        /// <summary>
        /// 获取用户登录凭据
        /// </summary>
        public static async Task<SessionResponse> GetSession(string code)
        {
            var url = $"{ApiHost}/sns/jscode2session?appid={MiniProgramSetting.AppId}&secret={MiniProgramSetting.AppSecret}&js_code={code}&grant_type=authorization_code";
            return await url.GetJsonAsync<SessionResponse>();
        }

        /// <summary>
        /// 获取用户手机号信息
        /// </summary>
        public static async Task<AjaxResult<UserPhoneResponse>> GetUserPhone(string code)
        {
            try
            {
                UserPhoneResponse response = null;
                await ApiInvoke(async accessToken =>
                {
                    var url = $"{ApiHost}/wxa/business/getuserphonenumber?access_token={accessToken}";
                    response = await url.PostJsonAsync(new { code }).ReceiveJson<UserPhoneResponse>();
                    return response;
                });
                if (response.IsError) return AjaxResult<UserPhoneResponse>.Error(response.ToString(), response);
                return AjaxResult<UserPhoneResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return AjaxResult<UserPhoneResponse>.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 调用微信API
        /// </summary>
        /// <returns></returns>
        internal static async Task ApiInvoke(Func<string, Task<WeixinResponse>> func, bool throwEx = true)
        {
            var accessToken = await GetAccessToken();
            var response = await func(accessToken);
            if (response.IsAccessTokenExpires)
            {
                await GetAccessTokenInner();
                accessToken = await GetAccessTokenString();
                response = await func(accessToken);
                if (throwEx && response.IsError) throw new MessageException(response.ToString());
            }
        }

        /// <summary>
        /// 获取AccessToken
        /// </summary>
        /// <returns></returns>
        private static async Task<string> GetAccessToken()
        {
            var accessToken = await GetAccessTokenString();
            if (!string.IsNullOrWhiteSpace(accessToken)) return accessToken;
            await GetAccessTokenInner();
            accessToken = await GetAccessTokenString();
            return accessToken;
        }

        /// <summary>
        /// 获取AccessToken
        /// </summary>
        /// <returns></returns>
        private static async Task<string> GetAccessTokenString()
        {
            var accessTokenHandle = HttpContext.RequestServices.GetService<IMiniProgramAccessTokenHandle>();
            return await accessTokenHandle.GetAsync();
        }

        /// <summary>
        /// 获取AccessToken
        /// </summary>
        /// <returns></returns>
        private static async Task GetAccessTokenInner()
        {
            var accessTokenHandle = HttpContext.RequestServices.GetService<IMiniProgramAccessTokenHandle>();
            await accessTokenHandle.SetAsync(null);
            await DistributedLockHelper.LockAsync($"{nameof(MiniProgramHelper)}_{nameof(GetAccessTokenInner)}", 120 * 1000, async isFail =>
            {
                if (isFail)
                {
                    await Task.Delay(3 * 1000);
                    return;
                }
                var url = $"{ApiHost}/cgi-bin/stable_token";
                var response = await url.PostJsonAsync(new
                {
                    grant_type = "client_credential",
                    appid = MiniProgramSetting.AppId,
                    secret = MiniProgramSetting.AppSecret,
                    force_refresh = false
                }).ReceiveJson<AccessTokenResponse>();
                if (response.IsError) throw new MessageException(response.ToString());
                response.SeExpiresTime();
                await accessTokenHandle.SetAsync(response);
            });
        }
    }
}