# 基于Scoket底层服务器

# 1.基本配置

## 1.1 日志服务

- 基于log4net

- 使用方法

  ```c#
  // Log.cs 封装日志类
  using log4net;
  namespace ITXCM
  {
      public static class Log
      {
          private static ILog log;
  
          public static void Init(string name) => log = LogManager.GetLogger(name);
  
          public static void Info(object message) => log.Info(message);
  
          public static void InfoFormat(string format, params object[] args) => log.InfoFormat(format, args);
  
          public static void Warning(object message) => log.Warn(message);
  
          public static void WarningFormat(string format, params object[] args) => log.WarnFormat(format, args);
  
          public static void Error(object message) => log.Error(message);
  
          public static void ErrorFormat(string format, params object[] args) => log.ErrorFormat(format, args);
  
          public static void Fatal(object message) => log.Fatal(message);
  
          public static void FatalFormat(string format, params object[] args) => log.FatalFormat(format, args);
      }
  }
  ```

  ```c#
  // 加载配置文件 并初始化
  FileInfo fi = new FileInfo("log4net.config");
  log4net.Config.XmlConfigurator.ConfigureAndWatch(fi);
  Log.Init("GameServer");
  Log.Info("Game Server Init");
  ```

  ```xml
  <!--配置文件log4net.config--> 
  <log4net>
      <root>
          <level value="DEBUG" />
          <appender-ref ref="RollingLogFileAppender" />
          <appender-ref ref="ColoredConsoleAppender" />
      </root>
      <appender name="LogFileAppender" type="log4net.Appender.RollingFileAppender">
          <param name="File" value="logs/cmm.log" />
          <param name="AppendToFile" value="true" />
          <rollingStyle value="Size" />
          <maxSizeRollBackups value="10" />
          <maximumFileSize value="1MB" />
          <staticLogFileName value="true" />
          <layout type="log4net.Layout.PatternLayout">
              <conversionPattern value="%date [%02thread] %-5level %logger: %message%newline" />
          </layout>
      </appender>
      <appender name="RollingLogFileAppender" type="log4net.Appender.RollingFileAppender">
          <!--定义文件存放位置-->
          <file value="Log\\" />
          <appendToFile value="true" />
          <rollingStyle value="Date" />
          <datePattern value="yyyyMMdd'.txt'" />
          <staticLogFileName value="false" />
          <param name="MaxSizeRollBackups" value="100" />
          <layout type="log4net.Layout.PatternLayout">
              <!--每条日志末尾的文字说明-->
              <!--输出格式-->
              <!--样例：2008-03-26 13:42:32,111 [10] INFO MainClass - info-->
              <conversionPattern value="%newline%date [%thread] [%-5level] [%logger] ：%message" />
          </layout>
      </appender>
      <appender name="ColoredConsoleAppender" type="log4net.Appender.ColoredConsoleAppender">
          <mapping>
              <level value="ERROR" />
              <foreColor value="Red, HighIntensity" />
          </mapping>
          <mapping>
              <level value="Warn" />
              <foreColor value="Yellow,HighIntensity" />
          </mapping>
          <mapping>
              <level value="Info" />
              <foreColor value="White" />
          </mapping>
          <mapping>
              <level value="Debug" />
              <foreColor value="Purple, HighIntensity" />
          </mapping>
  
          <layout type="log4net.Layout.PatternLayout">
              <conversionPattern value="%date [%thread] %-5level %logger: %message%newline" />
          </layout>
      </appender>
  </log4net>
  ```

## 1.2 消息协议

- 基于protobuf-net
- 协议生成工具生成协议

## 1.3 ET数据实体框架

- 基于EntityFramework

- 可视化编辑数据库

# 2. Unity客户端

## 2.1 通用核心dll

- NetClient 核心连接对象
  - 挂载填写端口 地址
- NetService
  - ServerDisConnectNoMsg：服务器断开 无消息在发
  - ServerDisconnectHasMsg：服务器断开 有消息在发
  - ServerConnectSucess：服务器连接/重连成功

## 2.2 Service层请求与发送

```c#
// TestService.cs
using ITXCM;
using SkillBridge.Message;
using System;
using UnityEngine;

public class TestServer : Singleton<TestServer>, IDisposable
{
    public void Init()
    { }

    public void Dispose()
    {
        // 取消订阅
        MessageDistributer.Instance.Unsubscribe<TestResponseProto>(RecveTestResponse);
    }

    public TestServer()
    {
        // 订阅指定协议消息
        MessageDistributer.Instance.Subscribe<TestResponseProto>(RecveTestResponse);
    }
    // 向服务器发送的这个协议
    public void SendTestRequest(string value)
    {
        // 实例化一个net消息 给指定的协议实例化 然后发送
        NetMessage msg = new NetMessage { Request = new NetMessageRequest() };
        msg.Request.testRequestProto = new TestRequestProto { testContent = value };
        NetService.Instance.CheckConnentAndSend(msg);
    }
    // 接受服务器发送的这个协议
    private void RecveTestResponse(object sender, TestResponseProto res)
    {
        Debug.LogFormat("TestResponseProto:{0}", res.Res);
    }
}
```

# 3. GameServer游戏服务器

## 3.1 Service层请求与发送

- 初始化该服务层

  ```c#
    TestServer.Instance.Init();
  ```

- 具体服务层

  ```c#
  using ITXCM;
  using SkillBridge.Message;
  using System;
  using UnityEngine;
  
  public class TestServer : Singleton<TestServer>, IDisposable
  {
      public void Init()
      { }
  
      public void Dispose()
      {
          // 取消订阅
          MessageDistributer.Instance.Unsubscribe<TestResponseProto>(RecveTestResponse);
      }
  
      public TestServer()
      {
          // 订阅指定协议消息
          MessageDistributer.Instance.Subscribe<TestResponseProto>(RecveTestResponse);
      }
      // 向服务器发送的这个协议
      public void SendTestRequest(string value)
      {
          // 实例化一个net消息 给指定的协议实例化 然后发送
          NetMessage msg = new NetMessage { Request = new NetMessageRequest() };
          msg.Request.testRequestProto = new TestRequestProto { testContent = value };
          NetService.Instance.CheckConnentAndSend(msg);
      }
      // 接受服务器发送的这个协议
      private void RecveTestResponse(object sender, TestResponseProto res)
      {
          Debug.LogFormat("TestResponseProto:{0}", res.Res);
      }
  }
  ```

- 消息分发

  ```c#
  namespace ITXCM
  {
      public class MessageDispatch<T> : Singleton<MessageDispatch<T>>
      {
          /// <summary>
          /// 响应协议分发
          /// </summary>
          /// <param name="sender"></param>
          /// <param name="message"></param>
          public void Dispatch(T sender, SkillBridge.Message.NetMessageResponse message)
          {
              if (message.testResponseProto != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.testResponseProto); }
          }
  
          /// <summary>
          /// 请求协议分发
          /// </summary>
          /// <param name="sender"></param>
          /// <param name="message"></param>
          public void Dispatch(T sender, SkillBridge.Message.NetMessageRequest message)
          {
              if (message.testRequestProto != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.testRequestProto); }
          }
      }
  }
  ```

  