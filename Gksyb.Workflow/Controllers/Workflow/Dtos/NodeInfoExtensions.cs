using Gksyb.Core.Auth;
using Gksyb.Model.WorkFlow;

namespace Gksyb.Workflow.Controllers.Workflow.Dtos
{
    public static class NodeInfoExtensions
    {
        public static IQuery<NodeInfo> SelectNodeInfo(this IJoinQuery<WF_NODE, WF_TASK> source) =>
            source.Select((node, task) => new NodeInfo
            {
                Id = node.ID,
                NodeName = node.NODE_TITLE,
                NodeStatus = node.NODE_STATUS,
                NodeReason = node.NODE_REASON,
                StartDate = node.CREATEDATE,
                ViewDate = node.VIEWDATE,
                FinishDate = node.FINISHDATE,
                Operator = node.NODE_USER,

                TaskId = task.ID,
                Creator = task.CREATEUSER,
                CreateDate = task.CREATEDATE,
                Title = task.FLOW_TITLE,
                TaskFinishFlag = task.FINISHDATE.HasValue ? "1" : "0",
                TaskFinishDate = task.FINISHDATE
            });

        public static IQuery<NodeInfo> SelectNodeInfo(this IJoinQuery<WF_HISTORY_NODE, WF_HISTORY_TASK> source) =>
            source.Select((node, task) => new NodeInfo
            {
                Id = node.ID,
                NodeName = node.NODE_TITLE,
                NodeStatus = node.NODE_STATUS,
                NodeReason = node.NODE_REASON,
                StartDate = node.CREATEDATE,
                ViewDate = node.VIEWDATE,
                FinishDate = node.FINISHDATE,
                Operator = node.NODE_USER,

                TaskId = task.ID,
                Creator = task.CREATEUSER,
                CreateDate = task.CREATEDATE,
                Title = task.FLOW_TITLE,
                TaskFinishFlag = task.FINISHDATE.HasValue ? "1" : "0",
                TaskFinishDate = task.FINISHDATE
            });

        public static IJoinQuery<WF_NODE, WF_TASK> CorpFilter(this IJoinQuery<WF_NODE, WF_TASK> source, UserSession user) =>
            source.Where(user.IsSuper ? ((node, task) => task.CORPID == user.Corp.CorpID) : ((node, task) => node.NODE_USERID == user.UserID));

        public static IJoinQuery<WF_HISTORY_NODE, WF_HISTORY_TASK> CorpFilter(this IJoinQuery<WF_HISTORY_NODE, WF_HISTORY_TASK> source, UserSession user) =>
            source.Where(user.IsSuper ? ((node, task) => task.CORPID == user.Corp.CorpID) : ((node, task) => node.NODE_USERID == user.UserID));
    }
}