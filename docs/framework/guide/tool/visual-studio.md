# Visual Studio

工欲善其事必先利其器！Visual Studio 及其插件提供了非常多代码辅助功能，启用这些功能和插件将大大提高开发效率，这里列举一些常见的配置，后续不断完善。

## browserlink（浏览器链接）
browserlink是VS针对SPA应用开发的保存文件就刷新浏览器的功能，但非单页应用开启此功能会导致开发过程效率降低。关闭的方法如下：
`工具`->`选项`
![browserlink](/images/browserlink-10.jpg)

![browserlink](/images/browserlink-20.jpg)

## 关闭浏览器窗口和调试程序联动
VS默认停止调试模式就会关闭浏览器，关闭浏览器也会停止调试模式，关闭方法如下:
![browser-dev](/images/browser-dev-10.jpg)

## Git
Visual Studio默认自带Git插件，但当安装了多个源码管理器，需要手动指定下，指定方式如下：
![git](/images/git-10.jpg)

![git](/images/git-20.jpg)

## CodeMaid
CodeMaid是主要用于清理和简化我们的编码。
- `扩展`->`管理扩展`右上角搜索CodeMaid进行安装。

- [CodeMaid](https://marketplace.visualstudio.com/items?itemName=SteveCadwallader.CodeMaidVS2022)下载安装。

> 由于扩展市场是国外服务器，可能出现下载慢或者下载不了的情况，可以用自己手机下载完后再将文件传给PC安装。

- 配置CodeMaid选项，建议开启保存自动清理。
![CodeMaid](/images/code-maid-30.jpg)

- 单个文件清理：在代码文件中右键弹出菜单，如下操作。
![CodeMaid](/images/code-maid-10.jpg)

- 整个项目清理：在项目右键弹出菜单，如下操作。
![CodeMaid](/images/code-maid-20.jpg)

## 开启内联参数提示

![inline](/images/inline-10.jpg)

![inline](/images/inline-20.jpg)

## 开启全局智能提示

![unimported](/images/unimported-10.jpg)

![unimported](/images/unimported-20.jpg)

## 中文智能提示

打开网站 [https://dotnet.microsoft.com/zh-cn/download/intellisense](https://dotnet.microsoft.com/zh-cn/download/intellisense) 下载对应的语言版本。

:::tip 配置教程

如果配置了不能显示中文，可以查看此篇教程 [https://blog.csdn.net/sD7O95O/article/details/103776077](https://blog.csdn.net/sD7O95O/article/details/103776077)

:::

:::tip 关于 `NET6` 的中文智能提示

因为官方不再提供本地化包了，详情可查看相关 Issue [https://github.com/dotnet/docs/issues/27283](https://github.com/dotnet/docs/issues/27283)

可以使用博客园网友 `@internalnet` 制作的本地化包 [https://www.cnblogs.com/internalnet/p/16185298.html](https://www.cnblogs.com/internalnet/p/16185298.html)

:::

![intellisense](/images/intellisense-10.png)

![intellisense](/images/intellisense-20.png)