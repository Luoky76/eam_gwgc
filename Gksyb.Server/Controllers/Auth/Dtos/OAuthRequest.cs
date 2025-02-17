using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Gksyb.Model.Core
{
    public class OAuthRequest<T>
    {
        internal const string KEY = "eokW6j8@DZfwFBMiIa7ghzELcKYSuyAR";
        private const double Expiry = 3 * 60;

        /// <summary>
        /// 应用编码
        /// </summary>
        [Required(ErrorMessage = "应用编码不能为空")]
        public string AppId { get; set; }

        /// <summary>
        /// 请求内容体
        /// </summary>
        [Required(ErrorMessage = "请求内容体不能为空")]
        public string Body { get; set; }

        /// <summary>
        /// 时戳 格式为：2022-05-06 12:48:50
        /// </summary>
        [Required(ErrorMessage = "时戳不能为空")]
        public DateTime? TimeStamp { get; set; }

        /// <summary>
        /// 签名 AppId + Body + TimeStamp(格式为：2022-05-06 12:48:50) + 密钥 做SM3 SM3编码为UTF-8
        /// </summary>
        public string Sign { get; set; }

        private T _data;

        /// <summary>
        /// 请求数据
        /// </summary>
        [JsonIgnore]
        public T Data
        {
            get
            {
                if (_data != null) return _data;
                _data = typeof(T) == typeof(string) ? Body.CastTo<T>() : Body.ToObject<T>();
                return _data;
            }
            set { _data = value; }
        }

        /// <summary>
        /// 请求IP
        /// </summary>
        [JsonIgnore]
        public string IP { get; set; }

        /// <summary>
        /// 用户代理
        /// </summary>
        [JsonIgnore]
        public string UA { get; set; }

        public string CalcuSign(string secret) => CryptographyHelper.GetSM3($"{AppId}{Body}{TimeStamp.Value:yyyy-MM-dd HH:mm:ss}{secret}");

        /// <summary>
        /// 模型检查
        /// </summary>
        public bool Check(string secret, string ips, bool isThrow = true) => CheckBody(secret, isThrow) && CheckIP(ips, isThrow);

        /// <summary>
        /// 参数检查
        /// </summary>
        public async Task<SYS_OAUTH> Check(HttpContext context)
        {
            context ??= Common.Static.HttpContext.Current;
            Init(context.Request);
            var dbContext = context.RequestServices.GetRequiredService<IDbContext>();
            return await Check(dbContext);
        }

        /// <summary>
        /// 参数检查
        /// </summary>
        public async Task<SYS_OAUTH> Check(IDbContext dbContext)
        {
            var model = await dbContext.Query<SYS_OAUTH>().Where(c => c.APPID == AppId && c.FLAG == "1").FirstOrDefaultAsync();
            MessageException.ThrowIf(model == null, $"找不到{AppId}的记录");
            var secret = CryptographyHelper.DecryptSM4(model.SECRET, KEY);
            Check(secret, model.IP);
            return model;
        }

        /// <summary>
        /// 参数检查
        /// </summary>
        /// <returns></returns>
        public bool CheckBody(string secret, bool isThrow = true)
        {
            var isFail = Math.Abs((TimeStamp.Value - DateTime.Now).TotalSeconds) > Expiry;
            if (isThrow && isFail) throw new MessageException($"{TimeStamp:yyyy-MM-dd HH:mm:ss}已过期");
            if (isFail) return false;
            var sign = CalcuSign(secret);
            isFail = Sign != sign;
            if (isThrow && isFail) throw new MessageException("参数检查失败");
            return !isFail;
        }

        /// <summary>
        /// 检查IP
        /// </summary>
        public bool CheckIP(string ips, bool isThrow = true)
        {
            var list = (ips ?? "").Split(",").DistinctAndOrderBy().ToList();
            var isFail = !list.Any(pattern => Regex.IsMatch(IP, pattern));
            if (isThrow && isFail) throw new MessageException($"{IP}不在白名单");
            return !isFail;
        }

        /// <summary>
        /// 初始赋值
        /// </summary>
        public void Init(HttpRequest request)
        {
            IP = request.GetRealIP();
            UA = request.GetUserAgent();
        }
    }
}