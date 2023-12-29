using DocumentFormat.OpenXml.Wordprocessing;
using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * 使用 _TYPE 为后缀的字段进行 hh:mm 时长格式的识别
 */

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
        public decimal? CONPLAN { get; set; }

        /// <summary>
        /// 施工准备 hh:mm
        /// </summary>
        [ImporterHeader(Name = "施工准备")]
        [Display(Name = "施工准备 hh:mm")]
        public string CONPLAN_TYPE {
            get
            {
                return CONPLAN.ToString();
            }
            set
            {
                try
                {
                    if (value == null) return;
                    //格式：mm hh:mm dd:hh:mm
                    string[] duration = value.Split(':', '：');
                    if (duration.Length == 1)
                    {
                        CONPLAN = decimal.Parse(duration[0]);
                    }
                    else if (duration.Length == 2)
                    {
                        CONPLAN = decimal.Parse(duration[0]) * 60 + decimal.Parse(duration[1]);
                    }
                    else if (duration.Length == 3)
                    {
                        CONPLAN = decimal.Parse(duration[0]) * 1440 + decimal.Parse(duration[1]) * 60 + decimal.Parse(duration[2]);
                    }
                }
                catch (Exception e)
                {
                    throw new MessageException("施工准备数据有误\n" + e.Message);
                }
            }
        }

        /// <summary>
        /// 挖泥时间
        /// </summary>
        public decimal? DREDGETIME { get; set; }

        /// <summary>
        /// 挖泥时间 hh:mm
        /// </summary>
        [ImporterHeader(Name = "挖泥时间")]
        [Display(Name = "挖泥时间 hh:mm")]
        public string DREDGETIME_TYPE
        {
            get
            {
                return DREDGETIME.ToString();
            }
            set
            {
                try
                {
                    if (value == null) return;
                    //格式：mm hh:mm dd:hh:mm
                    string[] duration = value.Split(':', '：');
                    if (duration.Length == 1)
                    {
                        DREDGETIME = decimal.Parse(duration[0]);
                    }
                    else if (duration.Length == 2)
                    {
                        DREDGETIME = decimal.Parse(duration[0]) * 60 + decimal.Parse(duration[1]);
                    }
                    else if (duration.Length == 3)
                    {
                        DREDGETIME = decimal.Parse(duration[0]) * 1440 + decimal.Parse(duration[1]) * 60 + decimal.Parse(duration[2]);
                    }
                }
                catch (Exception e)
                {
                    throw new MessageException("挖泥时间数据有误\n" + e.Message);
                }
            }
        }

        /// <summary>
        /// 航行时间
        /// </summary>
        public decimal? SAILTIME { get; set; }

        /// <summary>
        /// 航行时间 hh:mm
        /// </summary>
        [ImporterHeader(Name = "航行时间")]
        [Display(Name = "航行时间 hh:mm")]
        public string SAILTIME_TYPE
        {
            get
            {
                return SAILTIME.ToString();
            }
            set
            {
                try
                {
                    if (value == null) return;
                    //格式：mm hh:mm dd:hh:mm
                    string[] duration = value.Split(':', '：');
                    if (duration.Length == 1)
                    {
                        SAILTIME = decimal.Parse(duration[0]);
                    }
                    else if (duration.Length == 2)
                    {
                        SAILTIME = decimal.Parse(duration[0]) * 60 + decimal.Parse(duration[1]);
                    }
                    else if (duration.Length == 3)
                    {
                        SAILTIME = decimal.Parse(duration[0]) * 1440 + decimal.Parse(duration[1]) * 60 + decimal.Parse(duration[2]);
                    }
                }
                catch (Exception e)
                {
                    throw new MessageException("航行时间数据有误\n" + e.Message);
                }
            }
        }

        /// <summary>
        /// 检修时间
        /// </summary>
        public decimal? REPAIRTIME { get; set; }

        /// <summary>
        /// 检修时间 hh:mm
        /// </summary>
        [ImporterHeader(Name = "检修时间")]
        [Display(Name = "检修时间 hh:mm")]
        public string REPAIRTIME_TYPE
        {
            get
            {
                return REPAIRTIME.ToString();
            }
            set
            {
                try
                {
                    if (value == null) return;
                    //格式：mm hh:mm dd:hh:mm
                    string[] duration = value.Split(':', '：');
                    if (duration.Length == 1)
                    {
                        REPAIRTIME = decimal.Parse(duration[0]);
                    }
                    else if (duration.Length == 2)
                    {
                        REPAIRTIME = decimal.Parse(duration[0]) * 60 + decimal.Parse(duration[1]);
                    }
                    else if (duration.Length == 3)
                    {
                        REPAIRTIME = decimal.Parse(duration[0]) * 1440 + decimal.Parse(duration[1]) * 60 + decimal.Parse(duration[2]);
                    }
                }
                catch (Exception e)
                {
                    throw new MessageException("检修时间数据有误\n" + e.Message);
                }
            }
        }

        /// <summary>
        /// 天气影响
        /// </summary>
        public decimal? WEATHEREFFECT { get; set; }

        /// <summary>
        /// 天气影响 hh:mm
        /// </summary>
        [ImporterHeader(Name = "天气影响")]
        [Display(Name = "天气影响 hh:mm")]
        public string WEATHEREFFECT_TYPE
        {
            get
            {
                return WEATHEREFFECT.ToString();
            }
            set
            {
                try
                {
                    if (value == null) return;
                    //格式：mm hh:mm dd:hh:mm
                    string[] duration = value.Split(':', '：');
                    if (duration.Length == 1)
                    {
                        WEATHEREFFECT = decimal.Parse(duration[0]);
                    }
                    else if (duration.Length == 2)
                    {
                        WEATHEREFFECT = decimal.Parse(duration[0]) * 60 + decimal.Parse(duration[1]);
                    }
                    else if (duration.Length == 3)
                    {
                        WEATHEREFFECT = decimal.Parse(duration[0]) * 1440 + decimal.Parse(duration[1]) * 60 + decimal.Parse(duration[2]);
                    }
                }
                catch (Exception e)
                {
                    throw new MessageException("天气影响数据有误\n" + e.Message);
                }
            }
        }

        /// <summary>
        /// 其他停工
        /// </summary>
        public decimal? OTHERSTOP { get; set; }

        /// <summary>
        /// 其他停工 hh:mm
        /// </summary>
        [ImporterHeader(Name = "其他停工")]
        [Display(Name = "其他停工 hh:mm")]
        public string OTHERSTOP_TYPE
        {
            get
            {
                return OTHERSTOP.ToString();
            }
            set
            {
                try
                {
                    if (value == null) return;
                    //格式：mm hh:mm dd:hh:mm
                    string[] duration = value.Split(':', '：');
                    if (duration.Length == 1)
                    {
                        OTHERSTOP = decimal.Parse(duration[0]);
                    }
                    else if (duration.Length == 2)
                    {
                        OTHERSTOP = decimal.Parse(duration[0]) * 60 + decimal.Parse(duration[1]);
                    }
                    else if (duration.Length == 3)
                    {
                        OTHERSTOP = decimal.Parse(duration[0]) * 1440 + decimal.Parse(duration[1]) * 60 + decimal.Parse(duration[2]);
                    }
                }
                catch (Exception e)
                {
                    throw new MessageException("其他停工数据有误\n" + e.Message);
                }
            }
        }

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
        /// 柴油总日耗
        /// </summary>
        [ImporterHeader(Name = "柴油总日耗")]
        [Display(Name = "柴油总日耗")]
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
        public decimal? WAIT_WORK { get; set; }

        /// <summary>
        /// 待工 hh:mm
        /// </summary>
        [ImporterHeader(Name = "待工")]
        [Display(Name = "待工 hh:mm")]
        public string WAIT_WORK_TYPE
        {
            get
            {
                return WAIT_WORK.ToString();
            }
            set
            {
                try
                {
                    if (value == null) return;
                    //格式：mm hh:mm dd:hh:mm
                    string[] duration = value.Split(':', '：');
                    if (duration.Length == 1)
                    {
                        WAIT_WORK = decimal.Parse(duration[0]);
                    }
                    else if (duration.Length == 2)
                    {
                        WAIT_WORK = decimal.Parse(duration[0]) * 60 + decimal.Parse(duration[1]);
                    }
                    else if (duration.Length == 3)
                    {
                        WAIT_WORK = decimal.Parse(duration[0]) * 1440 + decimal.Parse(duration[1]) * 60 + decimal.Parse(duration[2]);
                    }
                }
                catch (Exception e)
                {
                    throw new MessageException("待工数据有误\n" + e.Message);
                }
            }
        }

        /// <summary>
        /// 作业时间
        /// </summary>
        public decimal? WORK_TIME { get; set; }

        /// <summary>
        /// 作业时间 hh:mm
        /// </summary>
        [ImporterHeader(Name = "作业时间")]
        [Display(Name = "作业时间 hh:mm")]
        public string WORK_TIME_TYPE
        {
            get
            {
                return WORK_TIME.ToString();
            }
            set
            {
                try
                {
                    if (value == null) return;
                    //格式：mm hh:mm dd:hh:mm
                    string[] duration = value.Split(':', '：');
                    if (duration.Length == 1)
                    {
                        WORK_TIME = decimal.Parse(duration[0]);
                    }
                    else if (duration.Length == 2)
                    {
                        WORK_TIME = decimal.Parse(duration[0]) * 60 + decimal.Parse(duration[1]);
                    }
                    else if (duration.Length == 3)
                    {
                        WORK_TIME = decimal.Parse(duration[0]) * 1440 + decimal.Parse(duration[1]) * 60 + decimal.Parse(duration[2]);
                    }
                }
                catch (Exception e)
                {
                    throw new MessageException("作业时间有误\n" + e.Message);
                }
            }
        }


        /// <summary>
        /// 锚泊时间
        /// </summary>
        public decimal? ANCHOR_TIME { get; set; }

        /// <summary>
        /// 锚泊时间 hh:mm
        /// </summary>
        [ImporterHeader(Name = "锚泊时间")]
        [Display(Name = "锚泊时间 hh:mm")]
        public string ANCHOR_TIME_TYPE
        {
            get
            {
                return ANCHOR_TIME.ToString();
            }
            set
            {
                try
                {
                    if (value == null) return;
                    //格式：mm hh:mm dd:hh:mm
                    string[] duration = value.Split(':', '：');
                    if (duration.Length == 1)
                    {
                        ANCHOR_TIME = decimal.Parse(duration[0]);
                    }
                    else if (duration.Length == 2)
                    {
                        ANCHOR_TIME = decimal.Parse(duration[0]) * 60 + decimal.Parse(duration[1]);
                    }
                    else if (duration.Length == 3)
                    {
                        ANCHOR_TIME = decimal.Parse(duration[0]) * 1440 + decimal.Parse(duration[1]) * 60 + decimal.Parse(duration[2]);
                    }
                }
                catch (Exception e)
                {
                    throw new MessageException("锚泊时间有误\n" + e.Message);
                }
            }
        }

        /// <summary>
        /// 主发电机运行时间
        /// </summary>
        public decimal? MAIN_RUNTIME { get; set; }

        /// <summary>
        /// 主发电机运行时间 hh:mm
        /// </summary>
        [ImporterHeader(Name = "主发电机运行时间")]
        [Display(Name = "主发电机运行时间 hh:mm")]
        public string MAIN_RUNTIME_TYPE
        {
            get
            {
                return MAIN_RUNTIME.ToString();
            }
            set
            {
                try
                {
                    if (value == null) return;
                    //格式：mm hh:mm dd:hh:mm
                    string[] duration = value.Split(':', '：');
                    if (duration.Length == 1)
                    {
                        MAIN_RUNTIME = decimal.Parse(duration[0]);
                    }
                    else if (duration.Length == 2)
                    {
                        MAIN_RUNTIME = decimal.Parse(duration[0]) * 60 + decimal.Parse(duration[1]);
                    }
                    else if (duration.Length == 3)
                    {
                        MAIN_RUNTIME = decimal.Parse(duration[0]) * 1440 + decimal.Parse(duration[1]) * 60 + decimal.Parse(duration[2]);
                    }
                }
                catch (Exception e)
                {
                    throw new MessageException("主发电机运行时间有误\n" + e.Message);
                }
            }
        }

        /// <summary>
        /// 主发电机累计时间
        /// </summary>
        public decimal? MAIN_CUMTIME { get; set; }

        /// <summary>
        /// 主发电机累计时间 hh:mm
        /// </summary>
        [ImporterHeader(Name = "主发电机累计时间")]
        [Display(Name = "主发电机累计时间 hh:mm")]
        public string MAIN_CUMTIME_TYPE
        {
            get
            {
                return MAIN_CUMTIME.ToString();
            }
            set
            {
                try
                {
                    if (value == null) return;
                    //格式：mm hh:mm dd:hh:mm
                    string[] duration = value.Split(':', '：');
                    if (duration.Length == 1)
                    {
                        MAIN_CUMTIME = decimal.Parse(duration[0]);
                    }
                    else if (duration.Length == 2)
                    {
                        MAIN_CUMTIME = decimal.Parse(duration[0]) * 60 + decimal.Parse(duration[1]);
                    }
                    else if (duration.Length == 3)
                    {
                        MAIN_CUMTIME = decimal.Parse(duration[0]) * 1440 + decimal.Parse(duration[1]) * 60 + decimal.Parse(duration[2]);
                    }
                }
                catch (Exception e)
                {
                    throw new MessageException("主发电机累计时间有误\n" + e.Message);
                }
            }
        }

        /// <summary>
        /// 停泊发电机运行时间
        /// </summary>
        public decimal? MOORING_RUNTIME { get; set; }

        /// <summary>
        /// 停泊发电机运行时间 hh:mm
        /// </summary>
        [ImporterHeader(Name = "停泊发电机运行时间")]
        [Display(Name = "停泊发电机运行时间 hh:mm")]
        public string MOORING_RUNTIME_TYPE
        {
            get
            {
                return MOORING_RUNTIME.ToString();
            }
            set
            {
                try
                {
                    if (value == null) return;
                    //格式：mm hh:mm dd:hh:mm
                    string[] duration = value.Split(':', '：');
                    if (duration.Length == 1)
                    {
                        MOORING_RUNTIME = decimal.Parse(duration[0]);
                    }
                    else if (duration.Length == 2)
                    {
                        MOORING_RUNTIME = decimal.Parse(duration[0]) * 60 + decimal.Parse(duration[1]);
                    }
                    else if (duration.Length == 3)
                    {
                        MOORING_RUNTIME = decimal.Parse(duration[0]) * 1440 + decimal.Parse(duration[1]) * 60 + decimal.Parse(duration[2]);
                    }
                }
                catch (Exception e)
                {
                    throw new MessageException("停泊发电机运行时间有误\n" + e.Message);
                }
            }
        }

        /// <summary>
        /// 停泊发电机累计时间
        /// </summary>
        public decimal? MOORING_CUMTIME { get; set; }

        /// <summary>
        /// 停泊发电机累计时间 hh:mm
        /// </summary>
        [ImporterHeader(Name = "停泊发电机累计时间")]
        [Display(Name = "停泊发电机累计时间 hh:mm")]
        public string MOORING_CUMTIME_TYPE
        {
            get
            {
                return MOORING_CUMTIME.ToString();
            }
            set
            {
                try
                {
                    if (value == null) return;
                    //格式：mm hh:mm dd:hh:mm
                    string[] duration = value.Split(':', '：');
                    if (duration.Length == 1)
                    {
                        MOORING_CUMTIME = decimal.Parse(duration[0]);
                    }
                    else if (duration.Length == 2)
                    {
                        MOORING_CUMTIME = decimal.Parse(duration[0]) * 60 + decimal.Parse(duration[1]);
                    }
                    else if (duration.Length == 3)
                    {
                        MOORING_CUMTIME = decimal.Parse(duration[0]) * 1440 + decimal.Parse(duration[1]) * 60 + decimal.Parse(duration[2]);
                    }
                }
                catch (Exception e)
                {
                    throw new MessageException("停泊发电机累计时间有误\n" + e.Message);
                }
            }
        }

        /// <summary>
        /// 主机运行时间
        /// </summary>
        public decimal? MAIN_ENGINE_RUNTIME { get; set; }

        /// <summary>
        /// 主机运行时间 hh:mm
        /// </summary>
        [ImporterHeader(Name = "主机运行时间")]
        [Display(Name = "主机运行时间 hh:mm")]
        public string MAIN_ENGINE_RUNTIME_TYPE
        {
            get
            {
                return MAIN_ENGINE_RUNTIME.ToString();
            }
            set
            {
                try
                {
                    if (value == null) return;
                    //格式：mm hh:mm dd:hh:mm
                    string[] duration = value.Split(':', '：');
                    if (duration.Length == 1)
                    {
                        MAIN_ENGINE_RUNTIME = decimal.Parse(duration[0]);
                    }
                    else if (duration.Length == 2)
                    {
                        MAIN_ENGINE_RUNTIME = decimal.Parse(duration[0]) * 60 + decimal.Parse(duration[1]);
                    }
                    else if (duration.Length == 3)
                    {
                        MAIN_ENGINE_RUNTIME = decimal.Parse(duration[0]) * 1440 + decimal.Parse(duration[1]) * 60 + decimal.Parse(duration[2]);
                    }
                }
                catch (Exception e)
                {
                    throw new MessageException("主机运行时间有误\n" + e.Message);
                }
            }
        }

        /// <summary>
        /// 主机累计时间
        /// </summary>
        public decimal? MAIN_ENGINE_CUMTIME { get; set; }

        /// <summary>
        /// 主机累计时间 hh:mm
        /// </summary>
        [ImporterHeader(Name = "主机累计时间")]
        [Display(Name = "主机累计时间 hh:mm")]
        public string MAIN_ENGINE_CUMTIME_TYPE
        {
            get
            {
                return MAIN_ENGINE_CUMTIME.ToString();
            }
            set
            {
                try
                {
                    if (value == null) return;
                    //格式：mm hh:mm dd:hh:mm
                    string[] duration = value.Split(':', '：');
                    if (duration.Length == 1)
                    {
                        MAIN_ENGINE_CUMTIME = decimal.Parse(duration[0]);
                    }
                    else if (duration.Length == 2)
                    {
                        MAIN_ENGINE_CUMTIME = decimal.Parse(duration[0]) * 60 + decimal.Parse(duration[1]);
                    }
                    else if (duration.Length == 3)
                    {
                        MAIN_ENGINE_CUMTIME = decimal.Parse(duration[0]) * 1440 + decimal.Parse(duration[1]) * 60 + decimal.Parse(duration[2]);
                    }
                }
                catch (Exception e)
                {
                    throw new MessageException("主机累计时间有误\n" + e.Message);
                }
            }
        }
    }
}
