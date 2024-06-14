namespace EAM.Material.DTO
{
    public class SP_APPLY_DET_DTO
    {
        /// <summary>
        /// 申请单号
        /// </summary>
        public string APPLY_NO { get; set; }

        /// <summary>
        /// 申请日期
        /// </summary>
        public DateTime? APPLY_DATE { get; set; }

        /// <summary>
        /// 申请人
        /// </summary>
        public string APPLY_USER { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string DEPT_NAME { get; set; }

        /// <summary>
        /// 申请主表ID
        /// </summary>
        public string APPLY_ID { get; set; }

        /// <summary>
        /// 物料ID
        /// </summary>
        public string SP_ID { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string SP_CODE { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        public string SP_NAME { get; set; }

        /// <summary>
        /// 型号规格
        /// </summary>
        public string SP_SIZE { get; set; }

        /// <summary>
        /// 品牌厂家
        /// </summary>
        public string PRODUCE { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string UNIT { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal? COUNT { get; set; }

        /// <summary>
        /// 库存数量
        /// </summary>
        public decimal? STORE_NUM { get; set; }

        /// <summary>
        /// 预估单价
        /// </summary>
        public decimal? YG_PRICE { get; set; }

        /// <summary>
        /// 预估金额
        /// </summary>
        public decimal? YG_MONEY { get; set; }

        /// <summary>
        /// 物料分类ID
        /// </summary>
        public string TYPE_ID { get; set; }

        /// <summary>
        /// 物料分类
        /// </summary>
        public string TYPE_NAME { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string MEMO { get; set; }

        /// <summary>
        /// 主键
        /// </summary>
        public string SPDET_ID { get; set; }

        /// <summary>
        /// 是否协议
        /// </summary>
        public string IS_XY { get; set; }

        /// <summary>
        /// 品牌无要求
        /// </summary>
        public string NO_PRODUCE { get; set; }

        /// <summary>
        /// 质保期
        /// </summary>
        public decimal? WARRANTY { get; set; }

        /// <summary>
        /// 物资状态(10计划,20待请购,30请购中,40采购中,50供货中,60质检待入库,70订单终止)
        /// </summary>
        public string SP_STATUS { get; set; }

        /// <summary>
        /// 采购状态
        /// </summary>
        public string AUDITING_CHECK { get; set; }
    }
}
