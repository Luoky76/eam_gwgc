using Azure;
using Flurl.Http;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.UI;
using Gksyb.Server.Services.Common.Dtos;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Gksyb.Server.Services.Common
{
    public class SmsService : ISmsService
    {
        private readonly LogPath _logPath = new("Sms");
        private readonly IBCCodeService _codeService;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<SmsService> _logger;
        private readonly short _codeLength = 6;

        public SmsService(IBCCodeService codeService, IDistributedCache distributedCache, ILogger<SmsService> logger)
        {
            _codeService = codeService;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task SendAsync(SmsInfo info)
        {
            var model = info.MapTo<SmsRequest>();
            string response = null;
            try
            {
                var request = Gksyb.Common.Static.HttpContext.Current?.Request;
                if (request != null)
                {
                    model.IP = request.GetRealIP();
                    model.UA = request.GetUserAgent();
                }
                else
                {
                    model.IP = model.Phone;
                    model.UA = model.Phone;
                }
                var code = await GetCodeAsync();
                model.Corp = code.TEXT;
                model.ComputeSign(code.VALUE);
                response = await $"{code.ID}".PostJsonAsync(model).ReceiveString();
                var result = response.ToObject<AjaxResult>();
                MessageException.ThrowIf(result.IsError, result.Message);
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                throw;
            }
            finally
            {
                _logger.LogInformation(_logPath, $"接到来自{model.IP}的【短信】请求，请求参数：{model.ToMiniJson()},应答数据：{response}");
            }
        }

        /// <inheritdoc/>
        public async Task<string> GenerateCodeAsync(string phone, int second = 120)
        {
            MessageException.ThrowIf(!phone.IsMobileNumber(), "请传递正确的手机号");
            var key = $"{phone}{nameof(SmsService)}";
            var value = await _distributedCache.GetStringAsync(key);
            MessageException.ThrowIf(!string.IsNullOrWhiteSpace(value), "上次验证码还在有效期中");
            var code = GuidHelper.NewSnowflakeId().ToString();
            code = code.Substring(code.Length - _codeLength, _codeLength);
            await SendAsync(new SmsInfo()
            {
                Phone = phone,
                TemplateCode = "SMS_241215702",
                TemplateParam = (new { code }).ToJson()
            });
            await _distributedCache.SetAsync(key, new KeyValueItem<int>() { Key = code, Value = 0 }, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(second)
            });
            return code;
        }

        /// <inheritdoc/>
        public async Task<int> CheckCodeAsync(string phone, string code)
        {
            var key = $"{phone}{nameof(SmsService)}";
            var model = await _distributedCache.GetAsync<KeyValueItem<int>>(key);
            if (model == null) return -1;
            if (code == model.Key)
            {
                await _distributedCache.RemoveAsync(key);
                return 0;
            }
            model.Value++;
            var times = 3;
            if (model.Value >= times)//输错三次去除
            {
                await _distributedCache.RemoveAsync(key);
                return -1;
            }
            await _distributedCache.SetAsync(key, model, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(120)
            });
            times -= model.Value;
            return times > 0 ? times : 1;
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        private async Task<ComboxData> GetCodeAsync()
        {
            if (_code != null) return _code;
            _code = (await _codeService.Get("短信配置", "短信配置")).FirstOrDefault() ?? new ComboxData()
            {
                ID = "http://172.16.201.13/zhgw/api/sms/send",
                TEXT = "GKSYB",
                VALUE = "smRqB7495CtE2O6iv@!ZSu8r1exlYX0j"
            };
            return _code;
        }

        private ComboxData _code;
    }
}