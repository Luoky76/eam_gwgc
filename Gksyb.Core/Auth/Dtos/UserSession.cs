using Gksyb.Common.Static;
using Gksyb.Core.Interfaces.Auth;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        /// <summary>
        /// 超级用户角色
        /// </summary>
        public static readonly string SuperRoleName = "$H^thKo#7E";

        /// <summary>
        /// Token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 班组
        /// </summary>
        public string Class { get; set; }

        /// <summary>
        /// 所属组织
        /// </summary>
        public CorpInfo Corp { get; set; }

        /// <summary>
        /// <see cref="Corp"/>的上级公司，如果<see cref="Corp"/>本身是公司，则取自己
        /// </summary>
        public CorpInfo ParentCompany { get; set; }

        /// <summary>
        /// 所属多组织
        /// </summary>
        public List<CorpInfo> Corps { get; set; }

        /// <summary>
        /// 所有组织（包含子组织）
        /// </summary>
        public List<CorpInfo> AllCorps { get; set; }

        /// <summary>
        /// 组 用于通知
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string Display
        {
            get
            {
                return $"{RealName}_{UserName}";
            }
        }

        /// <summary>
        /// 角色
        /// </summary>
        public List<string> Roles { get; set; }

        /// <summary>
        /// 是否超级管理员
        /// </summary>
        public bool IsSuper { get; set; }

        /// <summary>
        /// 是否管理员
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// 是否本公司
        /// </summary>
        public bool IsOurCompany { get; set; }

        /// <summary>
        /// 是否Api用户
        /// </summary>
        public bool IsApi { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 用户代理
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// 角色应用名
        /// </summary>
        public string UserAppName { get; set; }

        /// <summary>
        /// 角色应用名
        /// </summary>
        public string RoleAppName { get; set; }

        /// <summary>
        /// 菜单应用名
        /// </summary>
        public string MenuAppname { get; set; }

        /// <summary>
        /// 微信Openid
        /// </summary>
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
        public List<MenuModule> ForbinMenus { get; set; }

        /// <summary>
        /// 禁止的按钮权限
        /// </summary>
        public SortedList<string, List<ButtonModule>> ForbinButtons { get; set; }

        /// <summary>
        /// 扩展数据
        /// </summary>
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
                    new Claim(ClaimTypes.GroupSid, Group)
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
            var ip = request.GetRealIP();
            var userAgent = request.GetUserAgent();
            var times = 0;
            if (IP == ip) times += 1;
            if (UserAgent == userAgent) times += 1;
            if (times > 0 && Token == request.HttpContext.GetUID(false)) times += 1;
            if (times > 1) return true;
            distributedCache ??= request.HttpContext.RequestServices.GetService<IDistributedCache>();
            distributedCache.Remove(Token);
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
        /// 保存
        /// </summary>
        /// <returns></returns>
        public async Task<UserResponse> SaveAsync(SysContextOptions options)
        {
            Token = GuidHelper.NewShortId();
            await SaveAsync();
            HttpContext.Current.SetClientID(Token);
            var ticket = new string[]
            {
                $"{DateTime.Now.AddHours(options.RememberHours):yyyyMMddHHmmss}",
                Hash(UserAgent),
                IP,
                options.TicketVersion,
                UserName,
                MenuAppname,
                RoleAppName,
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
            MessageException.ThrowIf(ticketArray.Length != 8, "无效票据");
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
                ExtendData = ticketArray[7].ToObject<Dictionary<string, object>>()
            };
        }

        /// <summary>
        /// 临时存储票据过期时间
        /// </summary>
        [JsonIgnore]
        public DateTime? Expiration { get; set; }

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