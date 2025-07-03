using Gksyb.Common.Static;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Gksyb.Common.TCP
{
    public class BaseTcpServer : IDisposable
    {
        private Socket _listener = null;
        private readonly IPAddress _address;
        private readonly int _port;
        private readonly int _max;
        private readonly int _backlog;
        private ServerState serverState;
        private readonly ILogger _logger;
        private readonly LogPath _logPath;
        private readonly ConcurrentDictionary<ClientInfo, Socket> _clients = new();
        private readonly int _interval;
        private readonly Timer _timer;
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// 客户端超时时间 单位秒 -1代表从不超时
        /// </summary>
        public int ClientTimeOut = 3 * 60;

        /// <summary>
        /// TCP/IP 服务端
        /// </summary>
        /// <param name="port">端口</param>
        /// <param name="logPath">路径</param>
        /// <param name="max">最大连接数</param>
        /// <param name="interval">定时移除不活动连接的间隔时间，默认1分钟</param>
        /// <param name="backlog">最大排队数</param>
        /// <param name="address">IP地址，默认监听本机所有网卡</param>
        public BaseTcpServer(int port, LogPath logPath = null, int max = 10 * 1000, int interval = 60 * 1000, int backlog = 1000, IPAddress address = null)
        {
            _logger = HttpContext.RequestServices.GetRequiredService<ILogger<BaseTcpServer>>();
            _logPath = logPath ?? new LogPath($"TcpServer-{port}");
            _port = port;
            _max = max;
            _interval = interval;
            _backlog = backlog;
            _address = address ?? IPAddress.Any;
            _timer = new Timer(CheckInactiveClients, null, _interval, _interval);
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
        public event Func<ClientInfo, byte[], int, Socket, SocketError> OnReceive;

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

        /// <summary>
        /// 定时关闭不活动的连接
        /// </summary>
        private void CheckInactiveClients(object state)
        {
            if (ClientTimeOut <= 0) return;
            try
            {
                var now = DateTime.Now;
                foreach (var client in _clients.Keys)
                {
                    if (client.Socket?.Connected == true)
                    {
                        if ((client.ActieveTime ?? DateTime.MinValue).AddSeconds(ClientTimeOut) > now) continue;
                    }
                    RemoveClient(client);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(_logPath, ex.ToString());
            }
        }

        /// <summary>
        /// 监听
        /// </summary>
        private void Listen()
        {
            MessageException.ThrowIf(_disposed, "对象已释放");
            _listener = new Socket(_address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            var result = OnPrepareListen?.Invoke(_listener) ?? SocketError.Success;
            if (result != SocketError.Success) return;

            _logger?.LogError(_logPath, $"准备监听{_address}:{_port}");
            _listener.Bind(new IPEndPoint(_address, _port));
            _listener.Listen(_backlog);
            _logger?.LogError(_logPath, $"开始监听{_address}:{_port}");

            var eventArgs = new SocketAsyncEventArgs
            {
                UserToken = _listener
            };
            eventArgs.Completed += Accept_Completed;
            ListenAccept(_listener, eventArgs);
        }

        /// <summary>
        /// 监听客户端连接
        /// </summary>
        private void ListenAccept(Socket listener, SocketAsyncEventArgs e)
        {
            try
            {
                e.AcceptSocket = null;
                if (_disposed) return;
                if (listener?.AcceptAsync(e) == false)
                {
                    Accept_Completed(listener, e);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(_logPath, ex.ToString());
            }
        }

        /// <summary>
        /// 客户端连接
        /// </summary>
        private void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            var doNext = true;
            var listener = (Socket)e.UserToken;
            try
            {
                if (e.SocketError != SocketError.Success)
                {
                    switch (e.SocketError)
                    {
                        case SocketError.OperationAborted:
                            doNext = false;
                            _logger?.LogInformation(_logPath, $"主动关闭服务器");
                            break;

                        default:
                            _logger?.LogError(_logPath, $"{nameof(Accept_Completed)}操作{e.LastOperation}失败:{e.SocketError}");
                            break;
                    }
                    return;
                }
                if (e.LastOperation != SocketAsyncOperation.Accept) return;
                ProcessAccept(e);
            }
            catch (Exception ex)
            {
                e.AcceptSocket?.Dispose();//处理异常，关闭此次链接
                _logger?.LogError(_logPath, ex.ToString());
            }
            finally
            {
                if (doNext)
                    ListenAccept(listener, e);
            }
        }

        /// <summary>
        /// 处理客户端请求
        /// </summary>
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            if (e.AcceptSocket == null) return;
            Socket newSocket = e.AcceptSocket;
            if (_disposed)//对象如果释放，不再接收
            {
                newSocket.Dispose();
                return;
            }
            _logger?.LogInformation(_logPath, $"客户端{newSocket.RemoteEndPoint}连接服务器：{newSocket.LocalEndPoint}");
            if (_clients.Count > _max)
            {
                _logger?.LogError(_logPath, $"客户端数量已达到设定最大值{_max}，拒绝本次连接");
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

        /// <summary>
        /// 客户端成功连接处理
        /// </summary>
        protected virtual void Accept(ClientInfo client)
        {
            var result = OnAccept?.Invoke(client, _listener) ?? SocketError.Success;
            if (result != SocketError.Success || string.IsNullOrWhiteSpace(client.ID))
            {
                RemoveClient(client);
                return;
            }
            RemoveSameId(client);
            _clients[client] = client.Socket;
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
            var buffer = new byte[client.BufferLength];
            e.SetBuffer(buffer, 0, buffer.Length);
            if (client.Socket?.ReceiveAsync(e) == false)
            {
                IO_Completed(client.Socket, e);
            }
        }

        /// <summary>
        /// 接到客户端数据
        /// </summary>
        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            var doNext = true;
            var client = (ClientInfo)e.UserToken;
            try
            {
                if (e.SocketError != SocketError.Success || e.BytesTransferred <= 0)
                {
                    switch (e.SocketError)
                    {
                        case SocketError.Success:
                            _logger?.LogInformation(_logPath, $"已关闭的客户端{client.ID}");
                            break;

                        case SocketError.OperationAborted:
                            _logger?.LogInformation(_logPath, $"关闭客户端{client.ID}");
                            break;

                        case SocketError.ConnectionReset:
                            _logger?.LogInformation(_logPath, $"断开客户端{client.ID}");
                            break;

                        default:
                            _logger?.LogError(_logPath, $"{nameof(IO_Completed)}操作{e.LastOperation}失败:{e.SocketError}");
                            break;
                    }
                    doNext = false;
                    RemoveClient(client);
                    return;
                }
                if (e.LastOperation != SocketAsyncOperation.Receive) return;
                doNext = ProcessReceived(client, e);
            }
            catch (Exception ex)
            {
                _logger?.LogError(_logPath, ex.ToString());
            }
            finally
            {
                if (doNext)
                {
                    try
                    {
                        ListenReceive(client, e);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(_logPath, ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 处理接收的客户端数据
        /// </summary>
        private bool ProcessReceived(ClientInfo client, SocketAsyncEventArgs e)
        {
            client.ActieveTime = DateTime.Now;
            var buffer = new byte[e.BytesTransferred];
            Array.Copy(e.Buffer, 0, buffer, 0, e.BytesTransferred);
            _logger?.LogInformation(_logPath, $"接收来自{client.ID}的数据：{BitConverter.ToString(buffer)}");
            var result = SocketError.Success;
            var id = client.ID;
            if (client.Packet != null)
            {
                result = client.Packet.PackHandle(buffer, (bytes, remaining) =>
                {
                    return OnReceive?.Invoke(client, bytes, remaining, _listener) ?? SocketError.Success;
                });
            }
            else
            {
                result = OnReceive?.Invoke(client, buffer, 0, _listener) ?? SocketError.Success;
            }
            if (result != SocketError.Success)
            {
                RemoveClient(client);
                return false;
            }
            if (client.ID != id)//ID有发生变化
            {
                RemoveSameId(client);
            }
            return true;
        }

        /// <summary>
        /// 根据条件获取客户端
        /// </summary>
        private ClientInfo GetClient(Func<ClientInfo, bool> predicate) => _clients.Keys.OrderBy(c => c.AcceptTime).Last(predicate);

        /// <summary>
        /// 移除客户端
        /// </summary>
        private void RemoveClient(ClientInfo client)
        {
            try
            {
                if (client == null) return;
                if (_clients.TryRemove(client, out var _))
                {
                    OnClose?.Invoke(client, _listener);
                }
                client.Dispose();
                client = null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(_logPath, $"{client.ID}：{ex}");
            }
        }

        /// <summary>
        /// 移开除相同ID的客户端
        /// </summary>
        private void RemoveSameId(ClientInfo client)
        {
            _clients.Keys.Where(c => c.ID == client.ID && c != client).ForEach(c =>
            {
                RemoveClient(c);
            });
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
                var client = GetClient(c => c.ID == id);
                var result = OnSend?.Invoke(client, buffer, _listener) ?? SocketError.Success;
                if (result != SocketError.Success) return false;
                _logger?.LogInformation(_logPath, $"准备向{id}发送：{BitConverter.ToString(buffer)}");
                var times = 0;
                if (retry < 1) retry = 1;
                while (times < retry)
                {
                    times++;
                    try
                    {
                        client = GetClient(c => c.ID == id);
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
            Parallel.ForEach(_clients.Keys, client =>//开启多线程
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
                foreach (var client in _clients.Keys)
                {
                    RemoveClient(client);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                _listener?.Dispose();
                _listener = null;
                serverState = ServerState.Stopped;
            }
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
            _timer.Dispose();
            Stop();
            OnShutdown?.Invoke(_listener);
            _disposed = true;
        }
    }
}