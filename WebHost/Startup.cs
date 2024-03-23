using Chloe;
using Gksyb.Common;
using Gksyb.Common.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Serilog;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;

namespace WebHost
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly List<IPlugin> _plugins = new();

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
            var pluginDirectory = Path.Combine(AppContext.BaseDirectory, "plugins");
            if (!Directory.Exists(pluginDirectory)) return;
            //插件处理
            var pluginType = typeof(IPlugin);
            var loadedFileNames = AssemblyLoadContext.Default.Assemblies.Where(c => !c.IsDynamic && !string.IsNullOrWhiteSpace(c.Location))
                .Select(c => Path.GetFileNameWithoutExtension(c.Location)).ToList();
            var fileNames = Directory.GetFiles(pluginDirectory, "*.dll", SearchOption.TopDirectoryOnly);
            var businessAssemblies = new ConcurrentQueue<Assembly>();
            var pluginsPrefixs = configuration.GetSection(OptionName.PluginsPrefix).Get<List<string>>();
            var assemblyLoadContext = AssemblyLoadContext.Default;//new AssemblyLoadContext("GksybPlugins", false);
            Parallel.ForEach(fileNames, name =>//多线程加快效率
            {
                try
                {
                    var filename = Path.GetFileNameWithoutExtension(name);
                    if (loadedFileNames.Contains(filename)) return;//已加载的程序集不重复加载
                    var symbolFile = name.Replace("dll", "pdb");
                    Assembly assembly = null;
                    using var stream = File.OpenRead(name);
                    if (File.Exists(symbolFile))//加载符号文件，用于打印错误的代码行数
                    {
                        using var streamPdb = File.OpenRead(symbolFile);
                        assembly = assemblyLoadContext.LoadFromStream(stream, streamPdb);
                    }
                    else
                    {
                        assembly = assemblyLoadContext.LoadFromStream(stream);
                    }
                    if (!pluginsPrefixs.Any(c => assembly.FullName.StartsWith(c))) return;
                    businessAssemblies.Enqueue(assembly);
                }
                catch (Exception ex)
                {
                    Log.Error(name + "   " + ex.ToString());
                }
            });

            businessAssemblies.ForEach(assembly =>//加载完成后才能遍历
            {
                var types = assembly.GetTypes().Where(t => pluginType.IsAssignableFrom(t) && !t.IsAbstract);
                foreach (var type in types)
                {
                    var plugin = Activator.CreateInstance(type) as IPlugin;
                    _plugins.Add(plugin);
                }
            });
            _plugins = _plugins.OrderBy(c => c.GetOrder()).ToList();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.Use((context, next) =>
            {
                context.Request.EnableRewind();
                context.Response.AddSecurityHeader();//安全响应头
                return next.Invoke();
            });
            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            if (string.IsNullOrWhiteSpace(_env.WebRootPath))
            {
                var rootPath = Directory.GetCurrentDirectory();
                var index = rootPath.IndexOf("\\bin\\");
                _env.WebRootPath = Path.Combine(rootPath[..index], "wwwroot");
                _env.WebRootFileProvider = new PhysicalFileProvider(_env.WebRootPath);
            }
            _plugins.ForEach(plugin =>
            {
                plugin.PreConfigure(app, _env);
            });
            app.UseDefaultFiles();
            app.UseSafeStaticFiles(new StaticFileOptions()
            {
                ContentTypeProvider = new WebFileContentTypeProvider(_configuration.GetSection(OptionName.FileContentType).Get<Dictionary<string, string>>())
            });
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate = $"{{RequestMethod}} {{RequestPath}} responded {{StatusCode}} in {{Elapsed:0.0}} ms{Environment.NewLine}{{User}} {{IP}} {{UA}}{Environment.NewLine}Request:{{RequestBody}}{Environment.NewLine}Response:{{ResponseBody}}{Environment.NewLine}";
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set(nameof(LogPath), nameof(HttpContext));
                    diagnosticContext.Set("User", $"{httpContext.User?.Identity?.Name}");
                    diagnosticContext.Set("IP", $"{httpContext.Request.GetRealIP(true)}");
                    diagnosticContext.Set("UA", $"{httpContext.Request.GetUserAgent()}");
                    diagnosticContext.Set("RequestBody", httpContext.GetRequestBodyItem().SubStr(0, 1000));
                    diagnosticContext.Set("ResponseBody", httpContext.GetResponseBodyItem().SubStr(0, 200));
                };
            });
            app.UseRouting();
            _plugins.ForEach(plugin =>
            {
                plugin.Configure(app, _env);
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSysService(_configuration);
            var mvcBuilder = services.AddControllers(configure =>
            {
                configure.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;//禁止C# 8.0 验证非可空引用类型
                configure.MaxModelBindingCollectionSize = int.MaxValue;//最大绑定模型数
                configure.Filters.Add<ModelHandleFilter>();
                configure.Filters.Add<AjaxResultFilter>();
                configure.Conventions.Add(new ApplicationModelConvention());
            }).ConfigureApiBehaviorOptions(configure =>
            {
                configure.SuppressModelStateInvalidFilter = true;//禁用自动模型验证提示，已集成在ModelEncryptFilter
                configure.SuppressInferBindingSourcesForParameters = true;//禁用推理规则
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Custom();
            });
            //SignalR
            services.AddSignalR().AddRedis(_configuration).AddNewtonsoftJsonProtocol(configure =>//配置使用NewtonsoftJson
            {
                configure.PayloadSerializerSettings.Custom(igronNull: true);
            });
            _plugins.ForEach(plugin =>
            {
                plugin.ConfigureServices(services, mvcBuilder, _configuration);
            });
        }
    }
}