using Gksyb.Common.Static;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Gksyb.Common.TCP
{
    /// <summary>
    /// TCP客户端
    /// </summary>
    public class BaseTcpClient : IDisposable
    {
        private readonly ILogger _logger;
        private LogPath _logPath;
        private ClientInfo _client;
        private IPEndPoint _server;

        /// <summary>
        /// 标识
        /// </summary>
        public string ID { get; set; }

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// 断开连接，重连间隔（毫秒) null表示不重连
        /// </summary>
        public int? ReConnectTime { get; set; } = 3000;

        /// <summary>
        /// TCP客户端
        /// </summary>
        /// <param name="logPath">路径</param>
        public BaseTcpClient(string id = null, LogPath logPath = null)
        {
            ID = id;
            _logPath = logPath;
            _logger = HttpContext.RequestServices.GetRequiredService<ILogger<BaseTcpClient>>();
        }

        /// <summary>
        /// 连接服务器后
        /// </summary>
        public event Func<ClientInfo, SocketError> OnConnect;

        /// <summary>
        /// 接到数据
        /// </summary>
        public event Func<ClientInfo, byte[], SocketError> OnReceive;

        /// <summary>
        /// 发送数据
        /// </summary>
        public event Func<ClientInfo, byte[], SocketError> OnSend;

        /// <summary>
        /// 关闭连接
        /// </summary>
        public event Action<ClientInfo, SocketError> OnClose;

        /// <summary>
        /// 连接
        /// </summary>
        public void Connect(string ip, int port, bool reConnect = true)
        {
            _logPath ??= new LogPath($"TcpClient-{ip}-{port}");
            try
            {
                Connect(new IPEndPoint(IPAddress.Parse(ip), port));
            }
            catch (Exception)
            {
                if (!reConnect) throw;
                ReConnect();
            }
        }

        /// <summary>
        /// 建立连接
        /// </summary>
        private void Connect(IPEndPoint endPoint)
        {
            _server = endPoint;
            MessageException.ThrowIf(_disposed, "对象已释放");
            Close();
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _logger?.LogError(_logPath, $"准备连接：{_server}");
            socket.Connect(endPoint);
            _logger?.LogInformation(_logPath, $"客户端{socket.LocalEndPoint}连接服务器：{_server}");
            Accept(new ClientInfo()
            {
                ID = socket.LocalEndPoint.ToString(),
                Socket = socket,
            });
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            CloseClient(_client);
            _client = null;
        }

        /// <summary>
        /// 成功连接处理
        /// </summary>
        protected virtual void Accept(ClientInfo client)
        {
            _client = client;
            var result = OnConnect?.Invoke(client) ?? SocketError.Success;
            if (result != SocketError.Success || string.IsNullOrWhiteSpace(client.ID))
            {
                CloseClient(client);
                return;
            }
            var eventArgs = new SocketAsyncEventArgs()
            {
                UserToken = client
            };
            eventArgs.Completed += IO_Completed;
            ListenReceive(client, eventArgs);
        }

        /// <summary>
        /// 监听下一次数据接收
        /// </summary>
        private void ListenReceive(ClientInfo client, SocketAsyncEventArgs e)
        {
            try
            {
                client.ReceiveBuffer = new byte[client.BufferLength];
                e.SetBuffer(client.ReceiveBuffer, 0, client.ReceiveBuffer.Length);
                if (client.Socket?.ReceiveAsync(e) == false)
                {
                    IO_Completed(client.Socket, e);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(_logPath, ex.ToString());
            }
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            var doNext = true;
            var client = (ClientInfo)e.UserToken;
            try
            {
                if (e.SocketError == SocketError.OperationAborted && !Connected)
                {
                    doNext = false;
                    return;
                }
                if (e.SocketError != SocketError.Success)
                {
                    OnClose?.Invoke(client, e.SocketError);
                    switch (e.SocketError)
                    {
                        case SocketError.OperationAborted:
                            _logger?.LogInformation(_logPath, $"关闭同{_server}的连接{client?.ID}");
                            break;

                        case SocketError.ConnectionReset:
                            _logger?.LogInformation(_logPath, $"断开同{_server}的连接{client?.ID}");
                            ReConnect(ReConnectTime);
                            break;

                        default:
                            _logger?.LogError(_logPath, $"{nameof(IO_Completed)}操作{e.LastOperation}失败:{e.SocketError}");
                            ReConnect(ReConnectTime);
                            break;
                    }
                    doNext = false;
                    return;
                }
                if (e.LastOperation != SocketAsyncOperation.Receive || e.BytesTransferred <= 0) return;
                doNext = ProcessReceived(client, e);
            }
            catch (Exception ex)
            {
                _logger?.LogError(_logPath, ex.ToString());
            }
            finally
            {
                if (doNext)
                    ListenReceive(client, e);
            }
        }

        /// <summary>
        /// 处理接收的数据
        /// </summary>
        private bool ProcessReceived(ClientInfo client, SocketAsyncEventArgs e)
        {
            client.ActieveTime = DateTime.Now;
            var buffer = new byte[e.BytesTransferred];
            Array.Copy(client.ReceiveBuffer, 0, buffer, 0, e.BytesTransferred);
            client.ReceiveBuffer = buffer;
            _logger?.LogInformation(_logPath, $"接收来自{_server}的数据：{BitConverter.ToString(client.ReceiveBuffer)}");
            var result = SocketError.Success;
            if (client.Packet != null)
            {
                result = client.Packet.PackHandle(client.ReceiveBuffer, bytes =>
                {
                    return OnReceive?.Invoke(client, bytes) ?? SocketError.Success;
                });
            }
            else
            {
                result = OnReceive?.Invoke(client, client.ReceiveBuffer) ?? SocketError.Success;
            }
            if (result != SocketError.Success)
            {
                CloseClient(client);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        private void CloseClient(ClientInfo client)
        {
            try
            {
                if (client == null) return;
                client.Dispose();
            }
            catch (Exception ex)
            {
                _logger?.LogError(_logPath, $"{_client.ID}：{ex}");
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        public Task<bool> SendAsync(string buffer)
        {
            return SendAsync(Encoding.GetBytes(buffer));
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        public async Task<bool> SendAsync(byte[] buffer)
        {
            MessageException.ThrowIf(_server == null, "请先连接");
            try
            {
                if (ReConnect())
                {
                    await Task.Delay(1000);
                }
                var result = OnSend?.Invoke(_client, buffer) ?? SocketError.Success;
                if (result != SocketError.Success) return false;
                _logger?.LogInformation(_logPath, $"准备向{_server}发送：{BitConverter.ToString(buffer)}");
                if (await _client.Socket?.SendAsync(buffer, SocketFlags.None) == buffer.Length)
                {
                    _client.ActieveTime = DateTime.Now;
                    _logger?.LogInformation(_logPath, $"向{_server}发送成功");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(_logPath, ex.ToString());
            }
            _logger?.LogError(_logPath, $"向{_server}发送失败");
            return false;
        }

        /// <summary>
        /// 重新连接
        /// </summary>
        /// <returns></returns>
        private bool ReConnect(int? waitTime = null)
        {
            if (Connected) return false;
            lock (this)
            {
                if (Connected) return false;
                try
                {
                    Connect(_server);
                }
                catch (Exception ex)
                {
                    if (!waitTime.HasValue) throw;
                    _logger?.LogError(_logPath, $"重新连接失败，等待{waitTime}毫秒后重连，失败原因:{ex}");
                    Task.Delay(waitTime.Value).Result();
                    return ReConnect(waitTime);
                }
            }
            return true;
        }

        public bool Connected => _client?.Socket?.Connected == true;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed = false;

        private void Dispose(bool _)
        {
            if (_disposed) return;
            CloseClient(_client);
            _disposed = true;
        }
    }
}