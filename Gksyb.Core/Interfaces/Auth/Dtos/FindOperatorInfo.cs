namespace Gksyb.Core.Interfaces.Auth
{
    public class FindOperatorInfo
    {
        /// <summary>
        /// 查找类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 公司
        /// </summary>
        public string Corp { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string Operators { get; set; }

        /// <summary>
        /// 包含超管
        /// </summary>
        public bool HasSuper { get; set; }
    }
}