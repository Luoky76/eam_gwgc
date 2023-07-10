using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Gksyb.Common.TCP
{
    public class BaseTcpServer : IDisposable
    {
        private static Socket _listener = null;
        private readonly IPAddress _address;
        private readonly int _port;
        private readonly int _max;
        private readonly int _backlog;
        private ServerState serverState;
        private readonly ILogger _logger;
        private readonly LogPath _logPath;
        private readonly List<ClientInfo> _clients = new();
        private readonly double _interval;
        private System.Timers.Timer _timerCheckClient;
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// 客户端超时时间 单位秒 -1代表从不超时
        /// </summary>
        public int ClientTimeOut = 3 * 60;

        /// <summary>
        /// 定时器(业务逻辑)
        /// </summary>
        public System.Timers.Timer TimerCheckClient
        {
            get
            {
                if (_timerCheckClient == null)
                {
                    _timerCheckClient = new System.Timers.Timer
                    {
                        Interval = _interval
                    };
                    _timerCheckClient.Elapsed += TimerCheckClient_Elapsed;
                }
                return _timerCheckClient;
            }
        }

        /// <summary>
        /// TCP/IP 服务端
        /// </summary>
        /// <param name="logger">日志</param>
        /// <param name="port">端口</param>
        /// <param name="max">最大连接数</param>
        /// <param name="interval">定时移除不活动连接的间隔时间，默认1分钟</param>
        /// <param name="backlog">最大排队数</param>
        /// <param name="address">IP地址，默认监听本机所有网卡</param>
        /// <param name="logPath">路径</param>
        public BaseTcpServer(ILogger logger, int port, int max = 10 * 1000, double interval = 60 * 1000, int backlog = 1000, IPAddress address = null, LogPath logPath = null)
        {
            _logger = logger;
            _port = port;
            _max = max;
            _interval = interval;
            _backlog = backlog;
            _address = address ?? IPAddress.Any;
            _logPath = logPath ?? new LogPath("TcpServer");
        }

        /// <summary>
        /// 监听前
        /// </summary>
        public event Func<Socket, SocketError> OnPrepareListen;

        /// <summary>
        /// 有用户连接的时候
        /// </summary>
        public event Func<ClientInfo, Socket, SocketError> OnAccept;

        /// <summary>
        /// 接到数据
        /// </summary>
        public event Func<ClientInfo, byte[], Socket, SocketError> OnReceive;

        /// <summary>
        /// 发送数据
        /// </summary>
        public event Func<ClientInfo, byte[], Socket, SocketError> OnSend;

        /// <summary>
        /// 关闭连接
        /// </summary>
        public event Action<ClientInfo, Socket> OnClose;

        /// <summary>
        /// 关闭服务器
        /// </summary>
        public event Action<Socket> OnShutdown;

        /// <summary>
        /// 启动服务
        /// </summary>
        public virtual BaseTcpServer Start()
        {
            switch (serverState)
            {
                case ServerState.None:
                case ServerState.Stopped:
                    {
                        TimerCheckClient.Stop();
                        TimerCheckClient.Start();
                        Listen();
                        break;
                    }
                case ServerState.Running:
                    {
                        return this;
                    }
                case ServerState.Disposed:
                    {
                        throw new MessageException("无法重新利用已释放对象");
                    }
            }
            serverState = ServerState.Running;
            return this;
        }

        private bool IsCheckRun = false;

        /// <summary>
        /// 定时关闭不活动的连接
        /// </summary>
        private void TimerCheckClient_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (IsCheckRun) return;
            try
            {
                IsCheckRun = true;
                var now = DateTime.Now;
                for (var i = _clients.Count - 1; i >= 0; i--)
                {
                    var client = _clients[i];
                    if (client.Socket?.Connected == true)
                    {
                        if (ClientTimeOut < 0) continue;
                        if ((client.ActieveTime ?? DateTime.MinValue).AddSeconds(ClientTimeOut) > now) continue;
                    }
                    RemoveClient(client);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(_logPath, ex.ToString());
            }
            finally
            {
                IsCheckRun = false;
            }
        }

        private static readonly object _lockObj = new();

        /// <summary>
        /// 监听
        /// </summary>
        private void Listen()
        {
            try
            {
                lock (_lockObj)//同一时间不能建立多个socket
                {
                    _listener = new Socket(_address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    var result = OnPrepareListen?.Invoke(_listener) ?? SocketError.Success;
                    if (result != SocketError.Success) return;
                    _logger?.LogError(_logPath, $"准备监听{_address}:{_port}");
                    _listener.Bind(new IPEndPoint(_address, _port));

                    _listener.Listen(_backlog);
                    var acceptEventArg = new SocketAsyncEventArgs
                    {
                        UserToken = _listener
                    };
                    acceptEventArg.Completed += AcceptEventArg_Completed;
                    if (!_listener.AcceptAsync(acceptEventArg))
                    {
                        ProcessAccept(acceptEventArg);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(_logPath, $"在监听{_address}:{_port}时发生错误。{ex}");
            }
        }

        /// <summary>
        /// 客户端连接
        /// </summary>
        private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.Accept) return;
            ProcessAccept(e);
        }

        /// <summary>
        /// 处理客户端请求
        /// </summary>
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            if (_disposed) return;
            try
            {
                if (e.SocketError == SocketError.Success && e.AcceptSocket != null)
                {
                    try
                    {
                        Socket newSocket = e.AcceptSocket;
                        _logger?.LogInformation(_logPath, $"客户端{newSocket.RemoteEndPoint}连接服务器：{newSocket.LocalEndPoint}");
                        if (_clients.Count > _max)
                        {
                            _logger?.LogError(_logPath, $"客户端数量已达到设定最大值{_max}，拒绝本次连接");
                            newSocket.Close();
                            newSocket.Dispose();
                            return;
                        }
                        var clientInfo = new ClientInfo()
                        {
                            ID = newSocket.RemoteEndPoint.ToString(),
                            Socket = newSocket,
                        };
                        Accept(clientInfo);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(_logPath, ex.ToString());
                    }
                }
                e.AcceptSocket = null;
                if (!((Socket)e.UserToken).AcceptAsync(e))
                {
                    ProcessAccept(e);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 客户端连接
        /// </summary>
        protected virtual void Accept(ClientInfo client)
        {
            try
            {
                var result = OnAccept?.Invoke(client, _listener) ?? SocketError.Success;
                if (result != SocketError.Success || string.IsNullOrWhiteSpace(client.ID))
                {
                    RemoveClient(client);
                    return;
                }
                _clients.FindAll(c => c.ID == client.ID).ForEach(c =>
                {
                    RemoveClient(c);
                });
                client.ReceiveBuffer = new byte[client.BufferLength];
                _clients.Add(client);
                var readEventArgs = new SocketAsyncEventArgs();
                readEventArgs.Completed += IO_Completed;
                readEventArgs.UserToken = client;
                readEventArgs.SetBuffer(client.ReceiveBuffer, 0, client.ReceiveBuffer.Length);
                if (!client.Socket.ReceiveAsync(readEventArgs))
                {
                    ProcessReceived(readEventArgs);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(_logPath, ex.ToString());
            }
        }

        /// <summary>
        /// 接到客户端数据
        /// </summary>
        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.Receive) return;
            ProcessReceived(e);
        }

        /// <summary>
        /// 处理接收的数据
        /// </summary>
        private void ProcessReceived(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success || e.BytesTransferred <= 0) return;
            var client = e.UserToken as ClientInfo;
            try
            {
                client.ActieveTime = DateTime.Now;
                var buffer = new byte[e.BytesTransferred];
                Array.Copy(client.ReceiveBuffer, 0, buffer, 0, e.BytesTransferred);
                client.ReceiveBuffer = buffer;
                _logger?.LogInformation(_logPath, $"接收来自{client.ID}的数据：{Encoding.GetString(client.ReceiveBuffer)}");
                var result = SocketError.Success;
                if (client.Packet != null)
                {
                    result = client.Packet.PackHandle(client.ReceiveBuffer, bytes =>
                    {
                        return OnReceive?.Invoke(client, bytes, _listener) ?? SocketError.Success;
                    });
                }
                else
                {
                    result = OnReceive?.Invoke(client, client.ReceiveBuffer, _listener) ?? SocketError.Success;
                }
                if (result != SocketError.Success)
                {
                    RemoveClient(client);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(_logPath, ex.ToString());
            }
            try
            {
                client.ReceiveBuffer = new byte[client.BufferLength];
                e.SetBuffer(client.ReceiveBuffer, 0, client.ReceiveBuffer.Length);
                if (client.Socket?.ReceiveAsync(e) == false)
                {
                    ProcessReceived(e);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(_logPath, $"{client.ID}：{ex}");
            }
        }

        /// <summary>
        /// 移除客户端
        /// </summary>
        private void RemoveClient(ClientInfo client)
        {
            try
            {
                if (client == null) return;
                _logger?.LogInformation(_logPath, $"关闭客户端{client.ID}");
                OnClose?.Invoke(client, _listener);
                _clients.Remove(client);
                client.Dispose();
                client = null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(_logPath, $"{client.ID}：{ex}");
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        public Task<bool> SendAsync(string id, string buffer, int retry = 10, int delay = 2000)
        {
            return SendAsync(id, Encoding.GetBytes(buffer), retry, delay);
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        public Task<bool> SendAsync(string id, byte[] buffer, int retry = 10, int delay = 2000)
        {
            return SendAsync(id, buffer, 0, buffer.Length, retry, delay);
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        public async Task<bool> SendAsync(string id, byte[] buffer, int offset, int size, int retry = 10, int delay = 1000)
        {
            try
            {
                var client = _clients.FindLast(c => c.ID == id);
                var result = OnSend?.Invoke(client, buffer, _listener) ?? SocketError.Success;
                if (result != SocketError.Success) return false;
                _logger?.LogInformation(_logPath, $"准备向{id}发送：{Encoding.GetString(buffer)}");
                var times = 0;
                if (retry < 1) retry = 1;
                while (times < retry)
                {
                    times++;
                    try
                    {
                        client = _clients.FindLast(c => c.ID == id);
                        if (client == null)
                        {
                            await Task.Delay(delay);
                            continue;
                        }
                        if (client.Socket.Send(buffer, offset, size, SocketFlags.None) == size)
                        {
                            client.ActieveTime = DateTime.Now;
                            _logger?.LogInformation(_logPath, $"向{id}发送成功");
                            return true;
                        }
                    }
                    catch (Exception)
                    {
                    }
                    RemoveClient(client);
                    client = null;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(_logPath, ex.ToString());
            }
            _logger?.LogError(_logPath, $"向{id}发送失败");
            return false;
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        public void Broadcast(string data)
        {
            var buffer = Encoding.GetBytes(data);
            Parallel.ForEach(_clients, client =>//开启多线程
            {
                try
                {
                    client.Socket.Send(buffer);
                    client.ActieveTime = DateTime.Now;
                }
                catch (Exception)
                {
                }
            });
        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        public void Stop()
        {
            try
            {
                TimerCheckClient.Stop();
                for (var i = _clients.Count - 1; i >= 0; i--)
                {
                    var client = _clients[i];
                    RemoveClient(client);
                }
            }
            catch (Exception)
            {
            }
            _listener?.Dispose();
            _listener = null;
            serverState = ServerState.Stopped;
            TimerCheckClient.Dispose();
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
            _logger?.LogInformation(_logPath, $"关闭服务器{_listener.LocalEndPoint}");
            Stop();
            OnShutdown?.Invoke(_listener);
            _disposed = true;
        }
    }
}