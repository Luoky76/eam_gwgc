namespace Gksyb.Common
{
    public class SysContextOptions
    {
        /// <summary>
        /// 超级管理员ID
        /// </summary>
        public int AdminUserID { get; set; }

        /// <summary>
        /// 管理员用户组
        /// </summary>
        public int AdminRole { get; set; }

        /// <summary>
        /// 用户所在应用名称（用于多个系统共享用户）
        /// </summary>
        public string UserAppName { get; set; }

        /// <summary>
        /// 角色应用名称
        /// </summary>
        public string RoleAppName { get; set; }

        /// <summary>
        /// 应用名称
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// 应用名称
        /// </summary>
        public string MobileAppName { get; set; }

        /// <summary>
        /// 应用名称
        /// </summary>
        public string ConfigAppName { get; set; }

        /// <summary>
        /// 初始密码
        /// </summary>
        public string InitPassWord { get; set; }

        /// <summary>
        /// cookies记录时间
        /// </summary>
        public int RememberHours { get; set; }

        /// <summary>
        /// 票据版本号（可用于踢出所有用户）
        /// </summary>
        public string TicketVersion { get; set; }
    }
}