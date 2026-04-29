using Newtonsoft.Json;
using System.Collections;

namespace Gksyb.Common.Json
{
    /// <summary>
    /// 大型集合序列化处理(大于等于指定条数忽略null，否则走默认行为)
    /// </summary>
    public class LargeCollectionJsonConverter : JsonConverter
    {
        private readonly int NullValueIgnoreThreshold = 21;

        public LargeCollectionJsonConverter()
        {
        }

        public LargeCollectionJsonConverter(int nullValueIgnoreThreshold)
        {
            this.NullValueIgnoreThreshold = nullValueIgnoreThreshold;
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize(reader, objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (NullValueIgnoreThreshold > 0 && value is ICollection list && list.Count >= NullValueIgnoreThreshold)
            {
                writer.WriteRawValue(value.ToMiniJson());
                return;
            }
            serializer.Serialize(writer, value);
        }

    }
}
