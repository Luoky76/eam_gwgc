using System.Net.Sockets;

namespace Gksyb.Common.TCP
{
    /// <summary>
    /// 客户端信息
    /// </summary>
    public class ClientInfo : IDisposable
    {
        /// <summary>
        /// 主键
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 套接字
        /// </summary>
        public Socket Socket { get; set; }

        /// <summary>
        /// 接收缓冲区大小 默认 2K
        /// </summary>
        public int BufferLength { get; set; } = 2 * 1024;

        /// <summary>
        /// 接收缓冲区
        /// </summary>
        internal byte[] ReceiveBuffer { get; set; }

        /// <summary>
        /// 扩展属性
        /// </summary>
        public object Extra { get; set; }

        /// <summary>
        /// 上次活动时间
        /// </summary>
        public DateTime? ActieveTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 粘拆包信息
        /// </summary>
        public PacketInfo Packet { get; set; }

        public void Dispose()
        {
            Socket.Close();
            Socket.Dispose();
            Packet?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}