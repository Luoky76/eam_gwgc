namespace Gksyb.Model.UI
{
    /// <summary>
    /// 键值对
    /// </summary>
    public class KeyValueItem
    {
        /// <summary>
        /// 实际值
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 显示值
        /// </summary>
        public string Value { get; set; }
    }

    public class KeyValueItem<T>
    {
        /// <summary>
        /// 实际值
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 显示值
        /// </summary>
        public T Value { get; set; }
    }
}