using Gksyb.Common.Static;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Model.UI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Globalization;
using System.Security.Claims;

namespace Gksyb.Core.Auth
{
    /// <summary>
    /// 用户信息
    /// </summary>
    public partial class UserSession
    {
        private readonly LogPath _logPath = new(nameof(UserSession));
        private static readonly string VERSION;

        static UserSession()
        {
            var options = HttpContext.RequestServices.GetService<IOptions<SysContextOptions>>().Value;
            VERSION = options.TicketVersion;
        }

        /// <summary>
        /// 超级用户角色
        /// </summary>
        public const string SuperRoleName = "$H^thKo#7E";

        /// <summary>
        /// Token
        /// </summary>
        [JsonProperty("T")]
        public string Token { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        [JsonProperty("V")]
        public string Version { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [JsonProperty("UI")]
        public long UserID { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        [JsonProperty("UN")]
        public string UserName { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        [JsonProperty("RN")]
        public string RealName { get; set; }

        /// <summary>
        /// 班组
        /// </summary>
        [JsonProperty("C")]
        public string Class { get; set; }

        /// <summary>
        /// 工号 单点以此为凭据
        /// </summary>
        [JsonProperty("WC")]
        public string WorkerCode { get; set; }

        /// <summary>
        /// 所属组织
        /// </summary>
        public CorpInfo Corp { get; set; }

        /// <summary>
        /// <see cref="Corp"/>的上级公司，如果<see cref="Corp"/>本身是公司，则取自己
        /// </summary>
        [JsonProperty("PC")]
        public CorpInfo ParentCompany { get; set; }

        /// <summary>
        /// 所属多组织
        /// </summary>
        public List<CorpInfo> Corps { get; set; }

        /// <summary>
        /// 所有组织（包含子组织）
        /// </summary>
        [JsonProperty("AC")]
        public List<CorpInfo> AllCorps { get; set; }

        /// <summary>
        /// 组 用于通知
        /// </summary>
        [JsonProperty("G")]
        public string Group { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        [JsonIgnore]
        public string Display
        {
            get
            {
                return $"{RealName}_{UserName}";
            }
        }

        /// <summary>
        /// 所有角色
        /// </summary>
        [JsonProperty("AR")]
        public List<string> AllRoles { get; set; }

        /// <summary>
        /// 角色归属公司
        /// </summary>
        [JsonProperty("RC")]
        public List<KeyValueItem> RoleCorps { get; set; }

        /// <summary>
        /// 可用角色
        /// </summary>
        [JsonIgnore]
        public List<string> Roles
        {
            get
            {
                var allRoles = AllRoles ?? new List<string>();
                if (RoleCorps == null) return allRoles;
                return allRoles.Where(c =>
                {
                    var corps = RoleCorps.Where(a => a.Key == c).ToList();
                    if (corps.Count < 1) return true;
                    return corps.Any(a => a.Value == Corp.CorpID);
                }).ToList();
            }
        }

        /// <summary>
        /// 是否超级管理员
        /// </summary>
        [JsonProperty("IS")]
        public bool IsSuper { get; set; }

        /// <summary>
        /// 是否管理员
        /// </summary>
        [JsonProperty("IA")]
        public bool IsAdmin { get; set; }

        /// <summary>
        /// 是否本公司
        /// </summary>
        [JsonProperty("IOC")]
        public bool IsOurCompany { get; set; }

        /// <summary>
        /// 是否Api用户
        /// </summary>
        [JsonProperty("API")]
        public bool IsApi { get; set; }

        /// <summary>
        /// 是否开发者
        /// </summary>
        [JsonProperty("IDEV")]
        public bool IsDeveloper { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 用户代理
        /// </summary>
        [JsonProperty("UA")]
        public string UserAgent { get; set; }

        /// <summary>
        /// 角色应用名
        /// </summary>
        [JsonProperty("UAN")]
        public string UserAppName { get; set; }

        /// <summary>
        /// 角色应用名
        /// </summary>
        [JsonProperty("RAN")]
        public string RoleAppName { get; set; }

        /// <summary>
        /// 菜单应用名
        /// </summary>
        [JsonProperty("MAN")]
        public string MenuAppname { get; set; }

        /// <summary>
        /// 微信Openid
        /// </summary>
        [JsonIgnore]
        public string Openid
        {
            get
            {
                if (ExtendData.TryGetValue(nameof(Openid), out object value))
                {
                    return value as string;
                }
                return null;
            }
            set
            {
                ExtendData[nameof(Openid)] = value;
            }
        }

        /// <summary>
        /// 禁止的菜单权限
        /// </summary>
        [JsonProperty("FM")]
        public List<MenuModule> ForbinMenus { get; set; }

        /// <summary>
        /// 禁止的按钮权限
        /// </summary>
        [JsonProperty("FB")]
        public SortedList<string, List<ButtonModule>> ForbinButtons { get; set; }

        /// <summary>
        /// 扩展数据
        /// </summary>
        [JsonProperty("ED")]
        public Dictionary<string, object> ExtendData { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// 转化ClaimsPrincipal
        /// </summary>
        /// <returns></returns>
        public ClaimsPrincipal ToClaimsPrincipal()
        {
            var principal = new ClaimsPrincipal();
            principal.AddIdentity(new ClaimsIdentity(
               new[]
               {
                    new Claim(ClaimTypes.Sid, Token),
                    new Claim(ClaimTypes.NameIdentifier, UserID.ToString()),
                    new Claim(ClaimTypes.Name, UserName),
                    new Claim(ClaimTypes.GroupSid, Group ??""),
                    new Claim(ClaimTypes.DateOfBirth, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                    new Claim(ClaimTypes.UserData, $"【{Display}_{Group}_{Corp?.CName}】，IP：{IP}，UA：{UserAgent}")
               }, "GKSYB")
            {
                Label = RealName
            });
            return principal;
        }

        /// <summary>
        /// 判断IP和UA
        /// </summary>
        /// <returns></returns>
        public bool Check(Microsoft.AspNetCore.Http.HttpRequest request, IDistributedCache distributedCache = null)
        {
            if (VERSION == Version)
            {
                var ip = request.GetRealIP();
                var userAgent = request.GetUserAgent();
                var times = 0;
                if (IP == ip) times += 1;
                if (UserAgent == userAgent) times += 1;
                if (times > 0 && Token == request.HttpContext.GetUID(false)) times += 1;
                if (times > 1) return true;
                var logger = request.HttpContext.RequestServices.GetService<ILogger<UserSession>>();
                logger.LogError(_logPath, $"{Token}验证失败，{Environment.NewLine}当前：{ip}{Environment.NewLine}原始：{IP}{Environment.NewLine}当前：{userAgent}{Environment.NewLine}原始：{UserAgent}");
            }
            distributedCache ??= request.HttpContext.RequestServices.GetService<IDistributedCache>();
            if (!string.IsNullOrWhiteSpace(Token))
            {
                distributedCache.Remove(Token);
            }
            return false;
        }

        /// <summary>
        /// 转化用户返回
        /// </summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public UserResponse ToUserResponse(string ticket)
        {
            var response = this.MapTo<UserResponse>();
            if (Corp != null)
            {
                response.CorpId = Corp.CorpID;
                response.CorpName = Corp.CName;
            }
            response.Ticket = ticket;
            return response;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Group) && Corp != null && Corp.Station?.Count == 1)
            {
                Group = Corp.Station.FirstOrDefault();
            }
            var distributedCache = HttpContext.RequestServices.GetService<IDistributedCache>();
            return distributedCache.SetAsync(Token, this, new DistributedCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(20)
            });
        }

        /// <summary>
        /// 保存并返回票据
        /// </summary>
        /// <returns></returns>
        public async Task<UserResponse> SaveToTicketAsync()
        {
            var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
            var version = configuration.GetValue<string>($"{OptionName.SysContext}:{nameof(SysContextOptions.TicketVersion)}");
            var hours = configuration.GetValue<int?>($"{OptionName.SysContext}:{MenuAppname}_{nameof(SysContextOptions.RememberHours)}") ??
                configuration.GetValue<int?>($"{OptionName.SysContext}:{nameof(SysContextOptions.RememberHours)}") ?? 0;
            var expirationType = configuration.GetValue<string>($"{OptionName.SysContext}:{MenuAppname}_{nameof(ExpirationType)}") ??
                configuration.GetValue<string>($"{OptionName.SysContext}:{nameof(ExpirationType)}");
            Token = GuidHelper.NewShortId();
            Version = version;
            await SaveAsync();
            HttpContext.Current.SetClientID(Token);
            var ticket = new string[]
            {
                $"{DateTime.Now.AddHours(hours):yyyyMMddHHmmss}",
                Hash(UserAgent),
                IP,
                version,
                UserName,
                MenuAppname,
                RoleAppName,
                expirationType,
                ExtendData.ToJson()
            }.ToStr("@#");
            ticket = CryptographyHelper.EncryptSM4(ticket, KEY);
            return ToUserResponse(ticket);
        }

        /// <summary>
        /// ticket解析
        /// </summary>
        public static UserSession ParseTicket(string ticket, string version)
        {
            var ticketArray = CryptographyHelper.DecryptSM4(ticket, KEY).Split("@#");
            MessageException.ThrowIf(ticketArray.Length != 9, "无效票据");
            MessageException.ThrowIf(version != ticketArray[3], "无效票据");
            var expiration = DateTime.ParseExact(ticketArray[0], "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            MessageException.ThrowIf(expiration < DateTime.Now, "票据过时");
            return new UserSession()
            {
                Expiration = expiration,
                UserAgent = ticketArray[1],
                IP = ticketArray[2],
                UserName = ticketArray[4],
                MenuAppname = ticketArray[5],
                RoleAppName = ticketArray[6],
                ExpirationType = ticketArray[7],
                ExtendData = ticketArray[8].ToObject<Dictionary<string, object>>()
            };
        }

        /// <summary>
        /// 临时存储票据过期时间
        /// </summary>
        [JsonIgnore]
        public DateTime? Expiration { get; set; }

        /// <summary>
        /// 过期方式 1:绝对过期 其他:滑动过期
        /// </summary>
        [JsonIgnore]
        public string ExpirationType { get; set; }

        /// <summary>
        /// 密码加密
        /// </summary>
        public static string Encrypt(string password)
        {
            if (string.IsNullOrWhiteSpace(Cryptography))
            {
                var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
                Cryptography = configuration.GetValue($"{OptionName.SysContext}:Cryptography", defaultValue: "DES64");
            }
            return Cryptography switch
            {
                "SM" => CryptographyHelper.EncryptSM4(password, UKEY),
                _ => CryptographyHelper.Encrypt(password),
            };
        }

        public static string Hash(string value) => CryptographyHelper.GetSM3(value);

        private static string Cryptography;

        /// <summary>
        /// ticket密钥
        /// </summary>
        private const string KEY = "ASTWuovn2!x4j@awBbOGHch5RNylILF8iCKXqm1PVz9egJD#s7pM6ZdY30kfUtEQ";

        private const string UKEY = "yNJ1baz#5CLFiOPHIcDGh6xvQdWYR2uKpmnw8lEtVsgZo4fT@jXU3qBAMS7ek!0r";
    }
}