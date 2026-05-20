namespace Gksyb.Server.Services.Services.Dto
{
    public class GetConstructionEchartDataResponse
    {
        public List<decimal> FreshWaterCostList { get; set; }
        public List<decimal> DieselOilCostList { get; set; }
        public List<decimal> LubeCostList { get; set; }
    }
}
