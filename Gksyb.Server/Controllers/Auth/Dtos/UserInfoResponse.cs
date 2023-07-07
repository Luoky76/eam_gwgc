using Gksyb.Core.Interfaces.Auth;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace Gksyb.Model.Core
{
    public class UserInfoResponse
    {
        /// <summary>
        /// 用户标识
        /// </summary>
        public long? Id { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 工号
        /// </summary>
        public string WorkerCode { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 当前选择组织ID
        /// </summary>
        [JsonIgnore]
        public string CorpID { get; set; }

        /// <summary>
        /// 当前选择组织
        /// </summary>
        public CorpInfo Corp { get; set; }

        /// <summary>
        /// 用户关联的所有组织
        /// </summary>
        public List<CorpInfo> AllCorp { get; set; }

        /// <summary>
        /// cf_user=>UserInfoResponse
        /// </summary>
        public static readonly Expression<Func<CF_USER, UserInfoResponse>> FromCfUser = c => new UserInfoResponse()
        {
            Id = c.USERID,
            Account = c.LOGINNAME,
            Name = c.REALNAME,
            WorkerCode = c.DEPARTCODE,
            Phone = c.PHONE,
            CorpID = c.C_TERMINAL
        };
    }
}