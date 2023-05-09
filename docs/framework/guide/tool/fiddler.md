# Fiddler
Fiddler是位于客户端和服务器端之间的代理，也是目前最常用的抓包工具之一 。它能够记录客户端和服务器之间的所有请求，可以针对特定的请求，分析请求数据、设置断点、调试web应用、修改请求的数据，甚至可以修改服务器返回的数据，功能非常强大，是web调试的利器

## Fiddler工作原理
![fiddler](/images/fiddler-10.png)

抓包其中涉及三个角色： 客户端 、 代理、 目标服务器

原本正常访问网页或App路径是： 客户端 ---> 目标服务器
抓包，其实就加入了一个代理，相当于古代的 媒人 ，客户端 ---> 媒人（代理）-->目标服务器
只有这三者产生一定的联系，才能进行拦截/抓取一些东西，客户端先访问媒人，媒人把客户端的信息记录下来，媒人再去联系目标服务器，进而返回给客户端。

下载地址 [https://www.telerik.com/fiddler](https://www.telerik.com/fiddler)

使用文档 [https://www.jianshu.com/p/2ea4ce0fb945](https://www.jianshu.com/p/2ea4ce0fb945)