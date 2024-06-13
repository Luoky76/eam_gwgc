using System.Net.Sockets;

namespace Gksyb.Common.TCP
{
    public class PacketInfo : IDisposable
    {
        /// <summary>
        /// 包缓存
        /// </summary>
        internal List<byte> PackBuffer { get; set; } = new();

        /// <summary>
        /// 包缓存的最大长度 默认10K
        /// </summary>
        public int PackBufferMaxLength { get; set; } = 2 * 1024;

        /// <summary>
        /// 包头
        /// </summary>
        public int? PackHead { get; set; }

        /// <summary>
        /// 包头长度
        /// </summary>
        public int PackHeadSize { get; set; }

        /// <summary>
        /// 整个包长度（包含包头和包尾）
        /// </summary>
        public Func<IEnumerable<byte>, int> GetPackLength { get; set; }

        /// <summary>
        /// 粘拆包处理
        /// </summary>
        public SocketError PackHandle(byte[] data, Func<byte[], SocketError> handling)
        {
            var result = SocketError.Success;
            lock (PackBuffer)
            {
                PackBuffer.AddRange(data);
                var length = PackBuffer.Count;
                var index = 0;
                for (var i = 0; i < length; i++)
                {
                    if ((i + PackHeadSize - 1) >= length) break;
                    var bytes = PackBuffer.Skip(i).Take(PackHeadSize).ToArray();
                    if (BitConverter.ToUInt32(bytes) == PackHead)
                    {
                        var l = GetPackLength(PackBuffer.Skip(i + PackHeadSize));//包长度
                        var len = l + i;
                        if (len > length) break;
                        var handleResult = handling(PackBuffer.GetRange(i, l).ToArray());
                        result = handleResult != SocketError.Success ? handleResult : result;
                        index = len;
                        i = index - 1;
                    }
                }
                if (index < 1)
                {
                    if (PackBuffer.Count > PackBufferMaxLength)//大于最大包长度清空
                    {
                        PackBuffer.Clear();
                    }
                    return result;
                }
                PackBuffer.RemoveRange(0, index);
                return result;
            }
        }

        public void Dispose()
        {
            PackBuffer?.Clear();
            GC.SuppressFinalize(this);
        }
    }
}