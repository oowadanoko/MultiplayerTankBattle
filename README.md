# 多人坦克对战游戏
## 简介
&emsp;&emsp;本项目是在学习[罗培羽老师的《Unity3D网络游戏实战（第2版）》](https://github.com/luopeiyu/unity_net_book)时制作的多人坦克对战游戏。**但本项目的核心并不是在游戏而是在于网络**。本项目从底层的Socket通信开始，逐步实现了一个通用易用的网络框架，并使用该网络框架制作网络游戏。
> 本项目的实现与书中存在不符的地方。例如本项目服务端使用`System.Text.Json.JsonSerializer`类实现JSON数据的序列号与反序列号。该类是在.NET Core 3.0才引入的新类，更早的版本需要手动导入程序集。并且该类默认只序列化公共属性而不序列化字段，如要序列化字段需要在字段前加上特性`[JsonInclude]`。

因为重点是网络，所以游戏比较粗糙，比如动画、音效等都还没有做。~~（大概算个半成品吧）~~
## 框架介绍
> 从底层的Socket通信开始逐步实现了一个简单易用、具有一定通用性的网络框架。客户端使用完全使用异步方式，服务端使用Select多路复用方式。使用JSON格式进行数据通信。解决了网络通信中的常见的问题。
- 客户端
  - 完全使用异步方式进行通信
  - 实现了发送队列机制
    - 所有欲发送的数据都会先进入发送队列
    - 发送队列中的数据会按照入队顺序发送给服务器
  - 实现了消息队列机制
    - 所有接收到的消息会被先加入消息队列等待处理
    - 消息队列中的消息会依次调用消息处理函数进行处理
  - 解决了粘包半包问题
    - 发送的数据都会在开头加上长度，长度为一个16位整数，并手动确保了该整数以小端模式写入
    - 接收数据时根据开头的16位整数读取指定长度的数据，解决了粘包问题
    - 如果数据尚未全部送到则不会进行读取，解决了半包问题
  - 实现了心跳机制
    - 每隔一定时间向服务器发送ping消息，并等待服务器恢复pong消息
    - 如果长时间没有收到服务器回复则认为与服务器断开连接
- 服务端
  - 使用Select多路复用方式
    - 通过CheckRead列表检查可读的通信
  - 解决了粘包半包问题
    - 使用与客户端同样的技术解决粘包半包问题
  - 实现了心跳机制
    - 与客户端不同，服务器负责接收ping消息并回复pong消息
    - 如果长时间没有收到客户端的ping消息则认为客户端已断开连接
## 游戏介绍
> 实现了登录、注册、创建房间、加入房间以及实际游戏功能。在游戏中使用状态同步方式解决了客户端之间的同步问题。
- 客户端
  - 封装了简单的UI框架管理各个面板
  - 封装了资源加载方式
  - 实现了多个协议消息与服务器通信，并实现游戏中的状态同步
- 服务端
  - 实现了多个协议消息与客户端通信，并实现游戏中的状态同步
  - 封装了数据库操作
    - 对用户输入数据进行合法性检查，防止SQL注入攻击
- 数据库
  - 使用MySQL数据库
## 开发环境
- Windows 10
- Unity 2020.3
- Visual Studio 2019
- MySQL 8.0
- Navicat 15