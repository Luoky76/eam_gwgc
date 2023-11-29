using DocumentFormat.OpenXml.Wordprocessing;
using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAM.Special.DTO
{
    [ExcelExporter(Name = "通用导出测试", Author = "港口事业部", AutoFitMaxRows = 5000)]
    [ExcelImporter(MaxCount = 50000, HeaderRowIndex = 2)]
    public class BuildImportDto
    {
        /// <summary>
        /// 日期
        /// </summary>
        [ImporterHeader(Name = "日期")]
        [Display(Name = "日期")]
        public DateTime STARTDATE { get; set; }


        /// <summary>
        /// 船舶名称
        /// </summary>
        [ImporterHeader(Name = "船舶名称")]
        [Display(Name = "船舶名称")]
        public string DEVICE_NAME { get; set; }

        /// <summary>
        /// 船次
        /// </summary>
        [ImporterHeader(Name = "船次")]
        [Display(Name = "船次")]
        public int? SHIPTIMES { get; set; }

        /// <summary>
        /// 船方
        /// </summary>
        [ImporterHeader(Name = "船方")]
        [Display(Name = "船方")]
        public decimal? SHIPNUM { get; set; }

        /// <summary>
        /// 施工准备
        /// </summary>
        [ImporterHeader(Name = "施工准备")]
        [Display(Name = "施工准备")]
        public string CONPLAN { get; set; }

        /// <summary>
        /// 挖泥时间
        /// </summary>
        [ImporterHeader(Name = "挖泥时间")]
        [Display(Name = "挖泥时间")]
        public decimal? DREDGETIME { get; set; }

        /// <summary>
        /// 航行时间
        /// </summary>
        [ImporterHeader(Name = "航行时间")]
        [Display(Name = "航行时间")]
        public decimal? SAILTIME { get; set; }

        /// <summary>
        /// 检修时间
        /// </summary>
        [ImporterHeader(Name = "检修时间")]
        [Display(Name = "检修时间")]
        public decimal? REPAIRTIME { get; set; }

        /// <summary>
        /// 天气影响
        /// </summary>
        [ImporterHeader(Name = "天气影响")]
        [Display(Name = "天气影响")]
        public decimal? WEATHEREFFECT { get; set; }

        /// <summary>
        /// 其他停工
        /// </summary>
        [ImporterHeader(Name = "其他停工")]
        [Display(Name = "其他停工")]
        public decimal? OTHERSTOP { get; set; }

        /// <summary>
        /// 淡水日耗
        /// </summary>
        [ImporterHeader(Name = "淡水日耗")]
        [Display(Name = "淡水日耗")]
        public decimal? DAILYCONSUMPTION { get; set; }

        /// <summary>
        /// 淡水补充
        /// </summary>
        [ImporterHeader(Name = "淡水补充")]
        [Display(Name = "淡水补充")]
        public decimal? SUPPLEMENT { get; set; }

        /// <summary>
        /// 淡水库存
        /// </summary>
        [ImporterHeader(Name = "淡水库存")]
        [Display(Name = "淡水库存")]
        public decimal? STOCK { get; set; }

        /// <summary>
        /// 主机日耗
        /// </summary>
        [ImporterHeader(Name = "主机日耗")]
        [Display(Name = "主机日耗")]
        public decimal? MASTER { get; set; }

        /// <summary>
        /// 辅机日耗
        /// </summary>
        [ImporterHeader(Name = "辅机日耗")]
        [Display(Name = "辅机日耗")]
        public decimal? AUXILIARY { get; set; }

        /// <summary>
        /// 泵机日耗
        /// </summary>
        [ImporterHeader(Name = "泵机日耗")]
        [Display(Name = "泵机日耗")]
        public decimal? PUMP { get; set; }

        /// <summary>
        /// 小计
        /// </summary>
        [ImporterHeader(Name = "小计")]
        [Display(Name = "小计")]
        public decimal? SUBTOTAL { get; set; }

        

        /// <summary>
        /// 柴油补充
        /// </summary>
        [ImporterHeader(Name = "柴油补充")]
        [Display(Name = "柴油补充")]
        public decimal? SUPPLEMENT2 { get; set; }

        /// <summary>
        /// 滑油日耗
        /// </summary>
        [ImporterHeader(Name = "滑油日耗")]
        [Display(Name = "滑油日耗")]
        public decimal? LUBRICATE { get; set; }

        /// <summary>
        /// 柴油库存
        /// </summary>
        [ImporterHeader(Name = "柴油库存")]
        [Display(Name = "柴油库存")]
        public decimal? STOCK2 { get; set; }

        /// <summary>
        /// 简要说明
        /// </summary>
        [ImporterHeader(Name = "简要说明")]
        [Display(Name = "简要说明")]
        public string MEMO { get; set; }

        /// <summary>
        /// 待工
        /// </summary>
        [ImporterHeader(Name = "待工")]
        [Display(Name = "待工")]
        public decimal? WAIT_WORK { get; set; }

        /// <summary>
        /// 作业时间
        /// </summary>
        [ImporterHeader(Name = "作业时间")]
        [Display(Name = "作业时间")]
        public decimal? WORK_TIME { get; set; }


        /// <summary>
        /// 锚泊时间
        /// </summary>
        [ImporterHeader(Name = "锚泊时间")]
        [Display(Name = "锚泊时间")]
        public decimal? ANCHOR_TIME { get; set; }

        /// <summary>
        /// 主发电机运行时间
        /// </summary>
        [ImporterHeader(Name = "主发电机运行时间")]
        [Display(Name = "主发电机运行时间")]
        public decimal? MAIN_RUNTIME { get; set; }

        /// <summary>
        /// 主发电机累计时间
        /// </summary>
        [ImporterHeader(Name = "主发电机累计时间")]
        [Display(Name = "主发电机累计时间")]
        public decimal? MAIN_CUMTIME { get; set; }

        /// <summary>
        /// 停泊发电机运行时间
        /// </summary>
        [ImporterHeader(Name = "停泊发电机运行时间")]
        [Display(Name = "停泊发电机运行时间")]
        public decimal? MOORING_RUNTIME { get; set; }

        /// <summary>
        /// 停泊发电机累计时间
        /// </summary>
        [ImporterHeader(Name = "停泊发电机累计时间")]
        [Display(Name = "停泊发电机累计时间")]
        public decimal? MOORING_CUMTIME { get; set; }
    }
}
