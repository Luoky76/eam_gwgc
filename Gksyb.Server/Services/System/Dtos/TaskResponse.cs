using Chloe.Annotations;

namespace Gksyb.Model.Core
{
    [NotMapped]
    public class TaskResponse : SYS_TASK
    {
        /// <summary>
        /// 下次执行时间
        /// </summary>
        public DateTime? NextFireTime { get; set; }
    }
}