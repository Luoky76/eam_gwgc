using System.Collections.Generic;

namespace Gksyb.Model.Grid
{
    /// <summary>
    /// 通用保存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SaveRequest<T>
    {
        /// <summary>
        /// 新增
        /// </summary>
        public List<T> Added { get; set; }

        /// <summary>
        /// 修改
        /// </summary>
        public List<T> Updated { get; set; }

        /// <summary>
        /// 删除
        /// </summary>
        public List<T> Deleted { get; set; }

        /// <summary>
        /// 原始数据 修改前
        /// </summary>
        public List<T> Original { get; set; }
    }
}