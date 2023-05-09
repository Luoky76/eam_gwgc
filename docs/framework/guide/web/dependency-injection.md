# 依赖注入/控制反转

::: tip 阅前必读

如果对 `依赖注入/控制反转` 的架构理念理解不深，不明白 `作用域` 和 `多线程解析服务` 的问题，经常会因为不正确的使用导致内存不断飙高，正确的方式应该是：

- 尽可能的采用构造函数注入（如果这个类支持）
- 在非静态中（**但在 Web 请求有效的声明周期内**）可安全使用 `Gksyb.Common.Static.HttpContext.Current.RequestServices.GetRequiredService<>` 解析服务，如果是 `单例服务`，优先推荐构造函数注入或 `Gksyb.Common.Static.HttpContext.RequestServices.GetRequiredService<>` 方式
- 🤐 **在非 `Web` 环境、多线程环境、物联网等环境（含BackgroundService、定时任务等）🏒 除单例服务以外 🏒 必须采用 `IServiceScopeFactory.CreateAsyncScope()` 方式创建作用域且服务在内部委托中解析！** 🤐 想了解更多知识，可查阅 【(https://www.cnblogs.com/wucy/p/16566495.html)】

:::

## 依赖注入

所谓依赖注入，是指程序运行过程中，如果需要调用另一个对象协助时，无须在代码中创建被调用者，而是依赖于外部的注入。

通俗来讲，就是把有依赖关系的类放到容器中，然后在我们需要这些类时，容器自动解析出这些类的实例。

依赖注入最大的好处时实现类的解耦，利于程序拓展、单元测试、自动化模拟测试等。

依赖注入的英文为：`Dependency Injection`，简称 `DI`

## 控制反转

控制反转只是一个概念，也就是将创建对象实例的控制权（原本是程序员）从代码控制权剥离到 `IOC 容器` 中控制。

控制反转的英文为：`Inversion of Control`，简称 `IOC`

## `IOC/DI` 优缺点

传统的代码，每个对象负责管理与自己需要依赖的对象，导致如果需要切换依赖对象的实现类时，需要修改多处地方。同时，过度耦合也使得对象难以进行单元测试。

- 优点

  - 依赖注入把对象的创造交给外部去管理,很好的解决了代码紧耦合（tight couple）的问题，是一种让代码实现松耦合（loose couple）的机制
  - 松耦合让代码更具灵活性，能更好地应对需求变动，以及方便单元测试

- 缺点

  - 目前主流的 `IOC/DI` 基本采用反射的方式来实现依赖注入，在一定程度会影响性能

:::warning 特别说明

在本章节不打算细讲 `依赖注入/控制反转` 具体实现和应用场景，想了解更多知识，可查阅 【[ASP.NET Core 依赖注入](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-5.0)】 官方文档。

:::

## 依赖注入的三种方式

### 构造方法注入

目前构造方法注入是依赖注入推荐使用方式。

- 优点

  - 在构造方法中体现出对其他类的依赖，一眼就能看出这个类需要依赖哪些类才能工作
  - 脱离了 IOC 框架，这个类仍然可以工作，POJO 的概念
  - 一旦对象初始化成功了，这个对象的状态肯定是正确的

- 缺点

  - 构造函数会有很多参数（Bad smell）
  - 有些类是需要默认构造函数的，比如 MVC 框架的 Controller 类，一旦使用构造函数注入，就无法使用默认构造函数
  - 这个类里面的有些方法并不需要用到这些依赖（Bad smell）

代码示例：

```cs showLineNumbers  {4}
public class SampleService
{
    private readonly IDbContext _dbContext;
    public SampleService(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }
}
```

### 属性方式注入

:::danger 注意

ASP.NET不提供属性注入功能，建议使用构造函数或方法方式注入，也可以通过`HttpContext.RequestServices.GetRequiredService<>` 方式注入或者引入`AutoFac`。

:::

**通过属性方式注入容易和类的实例属性混淆，不建议使用。**

- 优点

  - 在对象的整个生命周期内，可以随时动态的改变依赖
  - 非常灵活

- 缺点

  - 对象在创建后，被设置依赖对象之前这段时间状态是不对的
  - 不直观，无法清晰地表示哪些属性是必须的

```cs showLineNumbers  {10}
public class SampleService
{
    private IDbContext _dbContext;

    public IDbContext DbContext
    {
        get
        {
            if (_dbContext != null) return _dbContext;
            _dbContext = HttpContext.RequestServices.GetRequiredService<IDbContext>();
            return _dbContext;
        }
    }
}
```

### 方法参数注入

方法参数注入的意思是在创建对象后，通过自动调用某个方法来注入依赖。

- 优点：

  - 比较灵活

- 缺点：

  - 新加入依赖时会破坏原有的方法签名，如果这个方法已经被其他很多模块用到就很麻烦
  - 与构造方法注入一样，会有很多参数

```cs showLineNumbers  {1}
public async Task<AjaxResult> ComboxDataAsync([FromServices] IComboxDataService comboxDataService)
{
    var data = await comboxDataService.Get(new Dictionary<string, object>(){
        { "BCCode", "职业" }
    });
    return AjaxResult.Success(data, "成功");
}
```

## 注册对象生存期

### `暂时/瞬时` AddTransient

暂时生存期服务是每次从服务容器进行请求时创建的。 这种生存期适合轻量级、 无状态的服务。

在处理请求的应用中，在请求结束时会释放暂时服务。

### `作用域` AddScoped

作用域生存期服务针对每个客户端请求（连接）创建一次。在处理请求的应用中，在请求结束时会释放有作用域的服务。

### `单例` AddSingleton

在首次请求它们时进行创建，之后每个后续请求都使用相同的实例。

:::tip 了解更多

想了解更多 `服务生存期` 知识可查阅 [ASP.NET Core - 依赖注入 - 服务生存期](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-5.0#service-lifetimes) 章节。

:::

### 插件依赖注册
项目的WebPlugin.cs负责依赖注册等其他功能，代码如下
```cs showLineNumbers  {21,26}

using Gksyb.Base.Interface;
using Gksyb.Core.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace XXX.Business
{
    internal class WebPlugin : IPlugin
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }

        public void ConfigureServices(IServiceCollection services, IMvcBuilder builder, IConfiguration configuration)
        {
            var assembly = Assembly.GetExecutingAssembly();
            assembly.AddIService((t, c) =>//注册IService
            {
                services.AddScoped(t, c);
            });

            assembly.AddIBaseService((t) =>//注册BaseService
            {
                services.AddScoped(t);
            });

            builder.AddApplicationPart(assembly);//MVC

            assembly.AddEntityTypeBuilder();//注册过滤器
        }
    }
}
```

## 示例

### 标准用法

创建 `ISampleService` 接口和 `SampleService` 实现类，代码如下：

```cs showLineNumbers  {4,9}

namespace XXX.Business
{
    public interface ISampleService : IService
    {
        SAMPLE_TABLE GetAsync(string id);
    }

    public class SampleService : ISampleService
    {
        private readonly IDbContext _dbContext;

        public SampleService(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public SAMPLE_TABLE GetAsync(string id)
        {
            return await _dbContext.QueryByKeyAsync<SAMPLE_TABLE>(id);
        }
    }
}
```

创建 `SampleController` 控制器，代码如下：

```cs showLineNumbers  {9,17}
using XXX.Business;
using Microsoft.AspNetCore.Mvc;

namespace XXX.Business.Controllers
{
    public class SampleController : AreaController
    {
        private readonly ISampleService _sampleService;
        public SampleController(ISampleService sampleService)
        {
            _sampleService = sampleService;
        }

        [HttpPost]
        public async Task<AjaxResult<SAMPLE_TABLE>> GetAsync(string id)
        {
            var entity = await _service.GetAsync(id);
            return AjaxResult<SAMPLE_TABLE>.Success(entity);
        }
    }
}
```

---

**例子解说**

框架提供了非常灵活且方便的实现依赖注入的方式，只需要接口类继承IService就可以作为被注入的接口。

上面的例子中，`SampleService` 注入了 `IDbContext` 仓储接口，同时 `SampleController` 控制器注入了 `ISampleService` 接口。

这样 `SampleController` 和 `SampleService` 之间就实现了解耦，不再依赖于具体的 `SampleService` 实例。

这就是依赖注入/控制反转最经典的例子。

### 泛型实例

创建 `ISampleService` 接口和 `SampleService` 实现类，代码如下：

```cs showLineNumbers  {3,8,12}
namespace XXX.Business
{
    public interface ISampleService : IService<SAMPLE_TABLE>
    {
        Task<List<ComboxData>> CorpData();
    }

    public class SampleService : BaseService<SAMPLE_TABLE>,ISampleService
    {
        private readonly IDbContext _dbContext;

        public SampleService(IDbContext dbContext): base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ComboxData>> CorpData()
        {
            return await _dbContext.Query<CF_CORP>()
                .Select(a => new ComboxData { ID = a.CORPID, TEXT = a.CORP_SNAME, VALUE = a.CORPID })
                .OrderBy(c => c.TEXT).ToListAsync();
        }
        
        /// <inheritdoc/>
        public override async Task<AjaxResult> SaveAsync(SaveRequest<SAMPLE_TABLE> request)
        {
           return await _dbContext.SaveEntityAnsyc(request,
                c => new { c.STRING_COLUMN, c.INT_COLUMN, c.FLOAT_COLUMN, c.DATE_COLUMN, c.COMB_COLUMN, c.DIFF_COMB_COLUMN, c.CORPID },
                c => a => a.SID == c.SID
                , BeforeAdd, BeforeUpdate, BeforeDelete, true);
        }
    }
}
```

创建 `SampleController` 控制器，代码如下：

```cs showLineNumbers  {9}
using XXX.Business;
using Microsoft.AspNetCore.Mvc;

namespace XXX.Business.Controllers
{
    public class SampleController : AreaController
    {
        private readonly ISampleService _sampleService;
        public SampleController(ISampleService sampleService)
        {
            _sampleService = sampleService;
        }

        [HttpPost]
        [HttpPost]
        public async Task<AjaxResult> ComboxDataAsync([FromServices] IComboxDataService comboxDataService)
        {
            var data = await comboxDataService.Get(new Dictionary<string, object>(){
                { "BCCode", "职业" }
            });
            data.TryAdd("corpData", await _sampleService.CorpData());
            return AjaxResult.Success(data, "成功");
        }

        [HttpPost]
        public async Task<AjaxResult<SAMPLE_TABLE>> GetAsync(string id)
        {
            var entity = await _sampleService.GetAsync(id);
            return AjaxResult<SAMPLE_TABLE>.Success(entity);
        }

        [HttpPost]
        public async Task<AjaxResult<GridData>> ListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _sampleService.ListAsync(request), "成功");
        }

        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> SaveAsync(SaveRequest<SAMPLE_TABLE> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _sampleService.SaveAsync(request);
        }
    }
}
```

### 无接口方式

有些时候，我们不想定义接口，而是想把实例类作为可依赖注入的对象，如 MVC 中的控制器。

创建 `SampleService` 实例类，代码如下：

```cs showLineNumbers  {4}

namespace XXX.Business
{
    public class SampleService : IBaseService
    {
        private readonly IDbContext _dbContext;

        public SampleService(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public SAMPLE_TABLE GetAsync(string id)
        {
            return await _dbContext.QueryByKeyAsync<SAMPLE_TABLE>(id);
        }
    }
}
```

创建 `SampleController` 控制器，代码如下：

```cs showLineNumbers  {9}
using XXX.Business;
using Microsoft.AspNetCore.Mvc;

namespace XXX.Business.Controllers
{
    public class SampleController : AreaController
    {
        private readonly SampleService _sampleService;
        public SampleController(SampleService sampleService)
        {
            _sampleService = sampleService;
        }

        [HttpPost]
        public async Task<AjaxResult<SAMPLE_TABLE>> GetAsync(string id)
        {
            var entity = await _service.GetAsync(id);
            return AjaxResult<SAMPLE_TABLE>.Success(entity);
        }
    }
}
```
