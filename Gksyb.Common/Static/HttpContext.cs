using Chloe.Reflection;
using Chloe.Reflection.Emit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;

namespace Gksyb.Common.Static
{
    public static class HttpContext
    {
        static HttpContext()
        {
            AddressList = new ReadOnlyCollection<string>(NetworkInterface.GetAllNetworkInterfaces()
                .Where(c => c.NetworkInterfaceType == NetworkInterfaceType.Ethernet && c.OperationalStatus == OperationalStatus.Up
                && !c.Description.ToLower().Contains("virtual") && !c.Description.ToLower().Contains("pseudo"))
                .SelectMany(p => p.GetIPProperties().UnicastAddresses)
                .Where(p => p.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(p.Address))
                .Select(c => c.Address.ToString()).Distinct().OrderBy(i => i).ToList());
        }

        private static IHttpContextAccessor _accessor;

        /// <summary>
        /// 添加全局HttpContext
        /// </summary>
        /// <returns></returns>
        public static IServiceCollection AddStaticHttpContext(this IServiceCollection services)
        {
            services.AddSingleton<IHostedService, HttpContextHostedService>();
            return services;
        }

        /// <summary>
        /// 初始化赋值
        /// </summary>
        internal static void Init(IServiceProvider source)
        {
            ResolvedServicesGetter = DelegateGenerator.CreateGetter(source.GetType().GetProperty("ResolvedServices",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public));
            RequestServices = source;
            _accessor = source.GetService<IHttpContextAccessor>();
        }

        public static Microsoft.AspNetCore.Http.HttpContext Current => _accessor?.HttpContext;

        /// <summary>
        /// 服务提供者
        /// </summary>
        public static IServiceProvider RequestServices { get; private set; }

        /// <summary>
        /// 地址列表
        /// </summary>
        public static ReadOnlyCollection<string> AddressList { get; private set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public static ushort Port { get; internal set; }

        private static string _address;

        /// <summary>
        /// Ip加端口
        /// </summary>
        public static string Address
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_address))
                {
                    _address = $"{AddressList.ToStr(",").SubStr(0, 495, true)}:{Port}";
                }
                return _address;
            }
        }

        /// <summary>
        /// 服务描述
        /// </summary>
        public static IServiceCollection ServiceCollection { get; set; }

        /// <summary>
        /// 映射服务获取器
        /// </summary>
        internal static MemberGetter ResolvedServicesGetter { get; set; }
    }

    internal class HttpContextHostedService : IHostedService
    {
        public HttpContextHostedService(IServiceProvider serviceProvider)
        {
            HttpContext.Init(serviceProvider);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}