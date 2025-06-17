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
        private bool _closed = false;
        private bool _connected = false;
        private bool _reConnect = false;

        private readonly SocketAsyncEventArgs _connectEventArgs;
        private readonly int _interval;
        private readonly Timer _timer;

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
        public BaseTcpClient(string id = null, LogPath logPath = null, int? interval = null)
        {
            ID = id;
            _logPath = logPath;
            _logger = HttpContext.RequestServices.GetRequiredService<ILogger<BaseTcpClient>>();
            _connectEventArgs = new SocketAsyncEventArgs { };
            _connectEventArgs.Completed += OnConnectCompleted;

            if (interval.HasValue)
            {
                _interval = interval.Value;
                _timer = new Timer(CheckInactive, null, _interval, _interval);
            }
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
        /// 服务器不活动，重连
        /// </summary>
        private void CheckInactive(object state)
        {
            try
            {
                if (!_connected) return;
                var now = DateTime.Now;
                var lastActieveTime = _client?.ActieveTime ?? now;
                if (lastActieveTime.AddMilliseconds(_interval) > now) return;
                _logger?.LogError(_logPath, $"{_interval}毫秒内未接到{_server}的数据，重新连接");
                Reconnect(SocketError.ConnectionReset);
            }
            catch (Exception ex)
            {
                _logger?.LogError(_logPath, ex.ToString());
            }
        }

        /// <summary>
        /// 连接
        /// </summary>
        public void Connect(string ip, int port)
        {
            _closed = false;
            _logPath ??= new LogPath($"TcpClient-{ip}-{port}");
            Connect(new IPEndPoint(IPAddress.Parse(ip), port));
        }

        private void Connect(IPEndPoint endPoint)
        {
            _connected = false;
            _server = endPoint;
            _connectEventArgs.RemoteEndPoint = _server;
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _logger?.LogError(_logPath, $"准备连接：{_server}");
            if (!socket.ConnectAsync(_connectEventArgs))
            {
                OnConnectCompleted(socket, _connectEventArgs);
            }
        }

        private void OnConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            _reConnect = false;
            if (e.SocketError != SocketError.Success)
            {
                _logger?.LogInformation(_logPath, $"连接服务器{_server}失败，原因：{e.SocketError}");
                Reconnect(e.SocketError);
                return;
            }
            _logger?.LogInformation(_logPath, $"客户端{e.ConnectSocket.LocalEndPoint}连接服务器：{_server}");
            _client = new ClientInfo()
            {
                ID = e.ConnectSocket.LocalEndPoint.ToString(),
                Socket = e.ConnectSocket,
            };
            var result = OnConnect?.Invoke(_client) ?? SocketError.Success;
            if (result != SocketError.Success || string.IsNullOrWhiteSpace(_client.ID))
            {
                Reconnect(result);
                return;
            }
            var buffer = new byte[_client.BufferLength];
            var receiveEventArgs = new SocketAsyncEventArgs();
            receiveEventArgs.SetBuffer(buffer, 0, buffer.Length);
            receiveEventArgs.Completed += OnReceiveCompleted;
            _connected = true;
            StartReceive(receiveEventArgs);
        }

        /// <summary>
        /// 监听下一次数据接收
        /// </summary>
        private void StartReceive(SocketAsyncEventArgs receiveEventArgs)
        {
            try
            {
                if (_client.Socket?.ReceiveAsync(receiveEventArgs) == false)
                {
                    OnReceiveCompleted(_client.Socket, receiveEventArgs);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(_logPath, ex.ToString());
            }
        }

        private void Reconnect(SocketError error)
        {
            CloseClient(error);
            if (!ReConnectTime.HasValue) return;
            if (_closed) return;
            if (_reConnect) return;
            _reConnect = true;
            Task.Delay(ReConnectTime.Value).ContinueWith(t =>
            {
                try
                {
                    Connect(_server);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(_logPath, ex.ToString());
                }
            });
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            _closed = true;
            CloseClient(SocketError.Success);
            _client = null;
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            var doNext = true;
            try
            {
                if (e.SocketError != SocketError.Success || e.BytesTransferred <= 0)
                {
                    doNext = false;
                    var error = e.SocketError switch
                    {
                        SocketError.Success => $"被动断开同{_server}的连接{_client?.ID}",
                        SocketError.OperationAborted => $"关闭同{_server}的连接{_client?.ID}",
                        SocketError.ConnectionReset => $"断开同{_server}的连接{_client?.ID}",
                        _ => $"{nameof(OnReceiveCompleted)}操作{e.LastOperation}失败:{e.SocketError}",
                    };
                    _logger?.LogError(_logPath, error);
                    Reconnect(e.SocketError);
                    return;
                }
                doNext = ProcessReceived(e);
            }
            catch (Exception ex)
            {
                _logger?.LogError(_logPath, ex.ToString());
            }
            finally
            {
                if (doNext)
                    StartReceive(e);
            }
        }

        /// <summary>
        /// 处理接收的数据
        /// </summary>
        private bool ProcessReceived(SocketAsyncEventArgs e)
        {
            _client.ActieveTime = DateTime.Now;
            var buffer = new byte[e.BytesTransferred];
            Buffer.BlockCopy(e.Buffer, e.Offset, buffer, 0, e.BytesTransferred);
            _logger?.LogInformation(_logPath, $"接收来自{_server}的数据：{BitConverter.ToString(buffer)}");
            var result = SocketError.Success;
            if (_client.Packet != null)
            {
                result = _client.Packet.PackHandle(buffer, bytes =>
                {
                    return OnReceive?.Invoke(_client, bytes) ?? SocketError.Success;
                });
            }
            else
            {
                result = OnReceive?.Invoke(_client, buffer) ?? SocketError.Success;
            }
            if (result == SocketError.Success)
                return true;
            Reconnect(result);
            return false;
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        private void CloseClient(SocketError error)
        {
            try
            {
                _connected = false;
                if (error != SocketError.Success)
                {
                    if (_client?.Socket?.Connected == true)
                    {
                        _logger?.LogInformation(_logPath, $"主动断开同{_server}的连接{_client?.ID}");
                    }
                    OnClose?.Invoke(_client, error);
                }
                _client?.Dispose();
            }
            catch (Exception ex)
            {
                _logger?.LogError(_logPath, $"{_client?.ID}：{ex}");
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
            try
            {
                var result = OnSend?.Invoke(_client, buffer) ?? SocketError.Success;
                if (result != SocketError.Success) return false;
                _logger?.LogInformation(_logPath, $"准备向{_server}发送：{BitConverter.ToString(buffer)}");
                MessageException.ThrowIf(_client?.Socket == null, "连接已断开");
                if (await _client.Socket.SendAsync(buffer, SocketFlags.None) == buffer.Length)
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed = false;

        private void Dispose(bool _)
        {
            if (_disposed) return;
            Close();
            _disposed = true;
        }
    }
}