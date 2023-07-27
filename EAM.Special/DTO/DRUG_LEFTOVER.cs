using System.ComponentModel;
using System.Data;

namespace EAM.Material.DTO
{
    public class DRUG_LEFTOVER
    {
        /// <summary>
        /// 药品物资ID
        /// </summary>
        [Description("主键")]
        public string SP_ID { get; set; }

        /// <summary>
        /// 剩余数量
        /// </summary>
        [Description("剩余数量")]
        public decimal? LEFTOVER { get; set; }
    }
}