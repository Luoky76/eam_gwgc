namespace Gksyb.Core.Interfaces.WorkFlow
{
    public class NodeStatus
    {
        /// <summary>
        /// 取消
        /// </summary>
        public const int Cancel = 0;

        /// <summary>
        /// 激活
        /// </summary>
        public const int Active = 1;

        /// <summary>
        /// 同意
        /// </summary>
        public const int Agree = 2;

        /// <summary>
        /// 拒绝
        /// </summary>
        public const int Reject = 3;

        /// <summary>
        /// 抄送
        /// </summary>
        public const int Share = 4;

        /// <summary>
        /// 退回
        /// </summary>
        public const int Back = 5;

        /// <summary>
        /// 转办
        /// </summary>
        public const int Transfer = 6;

        /// <summary>
        /// 已读
        /// </summary>
        public const int Readed = 8;

        /// <summary>
        /// 归档
        /// </summary>
        public const int Archived = 9;

        /// <summary>
        /// 草稿
        /// </summary>
        public const int Draft = 10;

        /// <summary>
        /// 退回归档
        /// </summary>
        public const int BackArchived = 11;

        /// <summary>
        /// 获取描述
        /// </summary>
        public static string GetDesc(int? status)
        {
            return status switch
            {
                Agree => "同意",
                Share => "抄送",
                Back => "退回",
                Transfer => "转办",
                Reject => "拒绝",
                Cancel => "取消",
                _ => null,
            };
        }
    }
}