using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gksyb.Server.Services.Services.Dto
{
    public class GetDeviceRepairInfoEchartResponse
    {
        public List<decimal> hourList { get; set; }
        public List<int> RepairList { get; set; }
    }
}
