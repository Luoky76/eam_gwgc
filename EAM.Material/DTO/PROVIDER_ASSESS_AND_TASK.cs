using System.ComponentModel;
using System.Data;

namespace EAM.Material.DTO
{
    public class PROVIDER_ASSESS_AND_TASK
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Description("主键")]
        public string ASSESS_ID { get; set; }

        /// <summary>
        /// 记录状态
        /// </summary>
        [Description("记录状态")]
        public string AUDITING { get; set; }

        /// <summary>
        /// 评估任务id
        /// </summary>
        [Description("评估任务id")]
        public string ASSESS_TASK_ID { get; set; }

        /// <summary>
        /// 评分人id
        /// </summary>
        [Description("评分人id")]
        public long EXAMINER_ID { get; set; }

        /// <summary>
        /// 考核说明
        /// </summary>
        [Description("考核说明")]
        public string REMARK { get; set; }

        /// <summary>
        /// 评价总分
        /// </summary>
        [Description("评价总分")]
        public double? TOTAL_SCORE { get; set; }

        /// <summary>
        /// 评价结果
        /// </summary>
        [Description("评价结果")]
        public string RESULT { get; set; }

        /// <summary>
        /// 供应商id
        /// </summary>
        [Description("供应商id")]
        public string PROVIDER_ID { get; set; }

        /// <summary>
        /// 供应商名
        /// </summary>
        [Description("供应商名")]
        public string PROVIDER_NAME { get; set; }

        /// <summary>
        /// 任务制定人id
        /// </summary>
        [Description("任务制定人id")]
        public long FORMULATER_ID { get; set; }

        /// <summary>
        /// 评估年度
        /// </summary>
        [Description("评估年度")]
        public string ASSESS_YEAR { get; set; }

        /// <summary>
        /// 考核开始时间
        /// </summary>
        [Description("考核开始时间")]
        public DateTime? BEGIN_TIME { get; set; }

        /// <summary>
        /// 考核结束时间
        /// </summary>
        [Description("考核结束时间")]
        public DateTime? END_TIME { get; set; }

        /// <summary>
        /// 供应商产品
        /// </summary>
        [Description("供应商产品")]
        public string PROVIDER_PRODUCTION { get; set; }
    }
}