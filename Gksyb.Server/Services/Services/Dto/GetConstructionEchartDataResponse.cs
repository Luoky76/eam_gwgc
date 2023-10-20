using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gksyb.Server.Services.Services.Dto
{
    public class GetConstructionEchartDataResponse
    {
        public List<decimal> FreshWaterCostList { get; set; }
        public List<decimal> DieselOilCostList { get; set; }
        public List<decimal> LubeCostList { get; set; }
    }
}
