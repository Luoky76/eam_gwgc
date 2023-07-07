using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Gksyb.Common
{
    /// <summary>
    /// XML帮助类
    /// </summary>
    public static class XMLHelper
    {
        public static string Serialize<T>(T obj, bool omitXmlDeclaration = true, Encoding encoding = null)
        {
            using var ms = new MemoryStream();
            encoding ??= Encoding.UTF8;
            if (encoding is UTF8Encoding)
            {
                encoding = new UTF8Encoding(false);//去除UTF8的BOM头
            }
            using var writer = XmlWriter.Create(ms, new XmlWriterSettings() { CloseOutput = true, OmitXmlDeclaration = omitXmlDeclaration, Encoding = encoding });
            var serializer = new XmlSerializer(obj.GetType());
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);
            serializer.Serialize(writer, obj, namespaces);
            return encoding.GetString(ms.ToArray());
        }

        public static T Deserialize<T>(string strXML, Encoding encoding = null) where T : class
        {
            encoding ??= Encoding.UTF8;
            using var ms = new MemoryStream(encoding.GetBytes(strXML));
            var serializer = new XmlSerializer(typeof(T));
            return serializer.Deserialize(ms) as T;
        }
    }
}