namespace Gksyb.Workflow.EventSubscriber.Dtos
{
    public static class WorkflowEventAction
    {
        /// <summary>
        /// 新增待办
        /// </summary>
        public const string AddTask = "WorkflowAddTask";

        /// <summary>
        /// 完成待办
        /// </summary>
        public const string ComplateTask = "WorkflowComplateTask";

        /// <summary>
        /// 新增待阅
        /// </summary>
        public const string AddShare = "WorkflowAddShare";
    }
}