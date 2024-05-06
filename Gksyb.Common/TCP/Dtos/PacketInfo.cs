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
        public byte[] PackHead { get; set; }

        /// <summary>
        /// 整个包长度（包含包头和包尾）
        /// </summary>
        public Func<IEnumerable<byte>, int> GetPackLength { get; set; }

        /// <summary>
        /// 是否同包头一致
        /// </summary>
        public bool IsEqualPackHead(int index)
        {
            var size = PackHead.Length;
            for (int i = 0; i < size; i++)
            {
                if (PackHead[i] != PackBuffer[index + i])
                    return false;
            }
            return true;
        }

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
                var size = PackHead.Length;
                for (var i = 0; i < length; i++)
                {
                    if ((i + size - 1) >= length) break;
                    if (IsEqualPackHead(i))
                    {
                        var l = GetPackLength(PackBuffer.Skip(i + size));//包长度
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