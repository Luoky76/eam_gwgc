using Gksyb.Core.Interfaces.WorkFlow;
using System.Linq.Expressions;

namespace Gksyb.Model.WorkFlow
{
    public class WF_NODEExtensions
    {
        /// <summary>
        /// 获取描述
        /// </summary>
        public static string GetDesc(int? status)
        {
            return status switch
            {
                NodeStatus.Agree => "同意",
                NodeStatus.Share => "抄送",
                NodeStatus.Back => "退回",
                NodeStatus.Transfer => "转办",
                NodeStatus.Reject => "拒绝",
                NodeStatus.Cancel => "取消",
                _ => null,
            };
        }

        /// <summary>
        /// 统计通过率状态
        /// </summary>
        private static readonly List<int?> PassRationStatus = new() { NodeStatus.Active, NodeStatus.Agree, NodeStatus.Reject };

        /// <summary>
        /// 统计通过率过滤
        /// </summary>
        public static Expression<Func<WF_NODE, bool>> PassRationFilter => node => PassRationStatus.Contains(node.NODE_STATUS);
    }
}