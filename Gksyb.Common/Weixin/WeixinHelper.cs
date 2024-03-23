using Flurl.Http;
using Gksyb.Common.Static;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Gksyb.Common.Weixin
{
    /// <summary>
    /// 应用授权作用域
    /// </summary>
    public enum OAuthScope
    {
        /// <summary>
        /// 不弹出授权页面，直接跳转，只能获取用户openid
        /// </summary>
        snsapi_base,

        /// <summary>
        /// 弹出授权页面，可通过openid拿到昵称、性别、所在地。并且，即使在未关注的情况下，只要用户授权，也能获取其信息
        /// </summary>
        snsapi_userinfo
    }

    public static class WeixinHelper
    {
        internal const string ApiHost = WeixinSetting.ApiHost;

        /// <summary>
        /// 获取验证地址
        /// </summary>
        /// <param name="redirectUrl">授权后重定向的回调链接地址</param>
        /// <param name="state">重定向后会带上state参数，开发者可以填写a-zA-Z0-9的参数值，最多128字节</param>
        /// <param name="scope">应用授权作用域，snsapi_base （不弹出授权页面，直接跳转，只能获取用户openid），snsapi_userinfo （弹出授权页面，可通过openid拿到昵称、性别、所在地。并且，即使在未关注的情况下，只要用户授权，也能获取其信息）</param>
        /// <returns></returns>
        public static string GetAuthorizeUrl(string redirectUrl, string state = null, OAuthScope scope = OAuthScope.snsapi_base)
        {
            if (string.IsNullOrWhiteSpace(state)) state = WeixinSetting.Token;
            var url = $"https://open.weixin.qq.com/connect/oauth2/authorize?appid={WeixinSetting.AppId}&redirect_uri={redirectUrl}&response_type=code&scope={scope:g}&state={state}#wechat_redirect";
            return url;
        }

        /// <summary>
        /// 获取AccessToken（OAuth专用）
        /// </summary>
        /// <param name="code">code作为换取access_token的票据，每次用户授权带上的code将不一样，code只能使用一次，5分钟未被使用自动过期。</param>
        /// <returns></returns>
        public static async Task<OAuthAccessTokenResponse> GetOauthAccessToken(string code)
        {
            var url = $"{ApiHost}/sns/oauth2/access_token?appid={WeixinSetting.AppId}&secret={WeixinSetting.AppSecret}&code={code}&grant_type=authorization_code";
            return await url.GetJsonAsync<OAuthAccessTokenResponse>();
        }

        /// <summary>
        /// 获取用户基本面信息
        /// </summary>
        /// <param name="openid">用户openid</param>
        /// <returns></returns>
        public static async Task<AjaxResult<UserInfoResponse>> GetUserInfo(string openid)
        {
            try
            {
                UserInfoResponse response = null;
                await ApiInvoke(async accessToken =>
                {
                    var url = $"{ApiHost}/cgi-bin/user/info?access_token={accessToken}&openid={openid}&lang=zh_CN";
                    response = await url.GetJsonAsync<UserInfoResponse>();
                    return response;
                });
                if (response.IsError) return AjaxResult<UserInfoResponse>.Error(response.ToString(), response);
                return AjaxResult<UserInfoResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return AjaxResult<UserInfoResponse>.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 发送模板消息
        /// </summary>
        /// <param name="openId">接收者openid</param>
        /// <param name="templateId">模板ID</param>
        /// <param name="templateUrl">模板跳转链接</param>
        /// <param name="data">数据</param>
        /// <param name="miniProgram">跳小程序所需数据，不需跳小程序可不用传该数据</param>
        /// <returns></returns>
        public static async Task<AjaxResult> SendTemplateMessage(string openId, string templateId, string templateUrl, string data, MiniProgramTemplateRequest miniProgram = null)
        {
            try
            {
                TemplateMessageResponse response = null;
                await ApiInvoke(async accessToken =>
                {
                    var url = $"{ApiHost}/cgi-bin/message/template/send?access_token={accessToken}";
                    var request = new TemplateRequest()
                    {
                        touser = openId,
                        template_id = templateId,
                        url = templateUrl,
                        data = data.ToObject<JToken>(),
                        miniprogram = miniProgram
                    };
                    response = await url.PostJsonAsync(request).ReceiveJson<TemplateMessageResponse>();
                    return response;
                });
                if (response.IsError) return AjaxResult.Error(response.ToString());
                return AjaxResult.Success(response.Msgid, response.ErrMsg);
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
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
            var accessTokenHandle = HttpContext.RequestServices.GetService<IAccessTokenHandle>();
            return await accessTokenHandle.GetAsync();
        }

        /// <summary>
        /// 获取AccessToken
        /// </summary>
        /// <returns></returns>
        private static async Task GetAccessTokenInner()
        {
            var accessTokenHandle = HttpContext.RequestServices.GetService<IAccessTokenHandle>();
            await accessTokenHandle.SetAsync(null);
            await DistributedLockHelper.LockAsync($"{nameof(WeixinHelper)}_{nameof(GetAccessTokenInner)}", 120 * 1000, async isFail =>
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
                    appid = WeixinSetting.AppId,
                    secret = WeixinSetting.AppSecret,
                    force_refresh = false
                }).ReceiveJson<AccessTokenResponse>();
                if (response.IsError) throw new MessageException(response.ToString());
                response.SeExpiresTime();
                await accessTokenHandle.SetAsync(response);
            });
        }
    }
}