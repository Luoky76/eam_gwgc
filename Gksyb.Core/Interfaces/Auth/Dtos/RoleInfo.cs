namespace Gksyb.Core.Interfaces.Auth
{
    /// <summary>
    /// 角色信息
    /// </summary>
    public class RoleInfo
    {
        /// <summary>
        /// 标识
        /// </summary>
        public long? Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// 组织
        /// </summary>
        public CorpInfo Corp { get; set; }
    }
}