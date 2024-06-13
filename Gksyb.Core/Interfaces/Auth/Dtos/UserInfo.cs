using Newtonsoft.Json;

namespace Gksyb.Core.Interfaces.Auth
{
    /// <summary>
    /// 人员信息
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// 用户标识
        /// </summary>
        public long? Id { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        [JsonIgnore]
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
        /// 组织
        /// </summary>
        public List<CorpInfo> Corps { get; set; } = new List<CorpInfo>();

        /// <summary>
        /// 岗位信息
        /// </summary>
        public string Station
        {
            get
            {
                if (Corps == null || Corps.Count < 1) return "";
                return Corps.Select(c =>
                {
                    if (c.Station == null || c.Station.Count < 1) return $"{c.CName}";
                    return c.Station.Select(a => $"{c.CName}-{a}").ToStr(",");
                }).ToStr(",");
            }
        }

        /// <summary>
        /// 默认分割符
        /// </summary>
        public const string DefaultSplit = ",";
    }
}