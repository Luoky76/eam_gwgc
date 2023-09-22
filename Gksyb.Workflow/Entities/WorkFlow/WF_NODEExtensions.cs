using Gksyb.Core.Interfaces.WorkFlow;
using Gksyb.Core.Interfaces.WorkFlow.Dtos;
using System.Linq.Expressions;

namespace Gksyb.Model.WorkFlow
{
    public static class WF_NODEExtensions
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

        /// <summary>
        /// WF_NODE转NodeInfo
        /// </summary>
        public static NodeInfo ToNodeInfo(this WF_NODE source)
        {
            return new NodeInfo()
            {
                Id = source.ID,
                NodeId = source.NODE_ID,
                NodeName = source.NODE_NAME,
                NodeTitle = source.NODE_TITLE,
                NodeType = source.NODE_TYPE,
                NodeStatus = source.NODE_STATUS,
                NodeUserId = source.NODE_USERID,
                NodeUserName = source.NODE_USERNAME,
                NodeUser = source.NODE_USER,
                NodeReason = source.NODE_REASON,
                Viewdate = source.VIEWDATE,
                Finishdate = source.FINISHDATE
            };
        }
    }
}