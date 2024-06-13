namespace Gksyb.Model.UI
{
    /// <summary>
    /// 键值对
    /// </summary>
    public class KeyValueItem
    {
        public KeyValueItem()
        {
        }

        public KeyValueItem(string key, string value)
        {
            Key = key;
            Value = value;
        }

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
        public KeyValueItem()
        {
        }

        public KeyValueItem(string key, T value)
        {
            Key = key;
            Value = value;
        }

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