namespace EAM.Special.DTO
{
    public class EnergyResponseDto
    {

        public string Generalcategory { get; set; }

        public string Subclass { get; set; }

        public string unit { get; set; }

        public decimal? January { get; set; } = 0;

        public decimal? February { get; set; } = 0;

        public decimal? March { get; set; } = 0;

        public decimal? April { get; set; } = 0;

        public decimal? May { get; set; } = 0;

        public decimal? June { get; set; } = 0;

        public decimal? July { get; set; } = 0;

        public decimal? August { get; set; } = 0;

        public decimal? September { get; set; } = 0;

        public decimal? October { get; set; } = 0;

        public decimal? November { get; set; } = 0;

        public decimal? December { get; set; } = 0;

        public decimal? SumNum { get; set; }

        /// <summary>
        /// 计算合计
        /// </summary>
        public void getTotal()
        {
            this.SumNum = this.January + this.February + this.March + this.April + this.May + this.June + this.July
                + this.August + this.September + this.October + this.November + this.December;
        }
    }
}
